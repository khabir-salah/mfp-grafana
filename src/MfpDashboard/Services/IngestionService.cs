using Dapper;
using MfpDashboard.Data;
using MfpDashboard.Models;
using Npgsql;

namespace MfpDashboard.Services;

public interface IIngestionService
{
    Task<UploadResult> IngestAsync(ParsedCsvData data, string fileName);
}

public class IngestionService : IIngestionService
{
    private readonly string _connectionString;
    private readonly ILogger<IngestionService> _logger;

    public IngestionService(DatabaseInitializer dbInit, ILogger<IngestionService> logger)
    {
        _connectionString = dbInit.ConnectionString;
        _logger = logger;
    }

    public async Task<UploadResult> IngestAsync(ParsedCsvData data, string fileName)
    {
        var result = new UploadResult { FileName = fileName, Warnings = data.Warnings };

        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            result.FoodRowsImported = await InsertFoodEntriesAsync(connection, data.FoodEntries);
            result.ExerciseRowsImported = await InsertExerciseEntriesAsync(connection, data.ExerciseEntries);
            result.WeightRowsImported = await InsertWeightEntriesAsync(connection, data.WeightEntries);

            if (data.DailySummaries.Any())
                await InsertDailySummariesAsync(connection, data.DailySummaries);

            // Recalculate daily summaries from food entries if we have food data
            if (data.FoodEntries.Any() || data.ExerciseEntries.Any())
                await RecalculateDailySummariesAsync(connection, data.FoodEntries, data.ExerciseEntries);

            await LogImportAsync(connection, fileName, data.DetectedType.ToString(),
                result.FoodRowsImported + result.ExerciseRowsImported + result.WeightRowsImported,
                data.Warnings, true);

            result.Success = true;
            result.Message = $"Successfully imported: {result.FoodRowsImported} food entries, " +
                             $"{result.ExerciseRowsImported} exercise entries, " +
                             $"{result.WeightRowsImported} weight entries.";

            _logger.LogInformation("Ingestion complete for {FileName}: {Message}", fileName, result.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ingestion failed for {FileName}", fileName);
            result.Success = false;
            result.Message = $"Ingestion failed: {ex.Message}";
            result.Warnings.Add(ex.Message);
        }

        return result;
    }

    private static async Task<int> InsertFoodEntriesAsync(NpgsqlConnection conn, List<FoodEntry> entries)
    {
        if (!entries.Any()) return 0;

        const string sql = """
            INSERT INTO food_entries 
                (date, meal_name, food_name, calories, carbohydrates, fat, protein, 
                 cholesterol, sodium, sugar, fiber)
            VALUES 
                (@Date, @MealName, @FoodName, @Calories, @Carbohydrates, @Fat, @Protein,
                 @Cholesterol, @Sodium, @Sugar, @Fiber)
            ON CONFLICT DO NOTHING
            """;

        var count = 0;
        foreach (var entry in entries)
        {
            count += await conn.ExecuteAsync(sql, entry);
        }
        return count;
    }

    private static async Task<int> InsertExerciseEntriesAsync(NpgsqlConnection conn, List<ExerciseEntry> entries)
    {
        if (!entries.Any()) return 0;

        const string sql = """
            INSERT INTO exercise_entries 
                (date, exercise_name, minutes, calories_burned, sets, reps, weight)
            VALUES 
                (@Date, @ExerciseName, @Minutes, @CaloriesBurned, @Sets, @Reps, @Weight)
            """;

        var count = 0;
        foreach (var entry in entries)
        {
            count += await conn.ExecuteAsync(sql, entry);
        }
        return count;
    }

    private static async Task<int> InsertWeightEntriesAsync(NpgsqlConnection conn, List<WeightEntry> entries)
    {
        if (!entries.Any()) return 0;

        const string sql = """
            INSERT INTO weight_entries (date, weight, unit)
            VALUES (@Date, @Weight, @Unit)
            ON CONFLICT (date) DO UPDATE 
                SET weight = EXCLUDED.weight, 
                    unit = EXCLUDED.unit,
                    imported_at = NOW()
            """;

        var count = 0;
        foreach (var entry in entries)
        {
            count += await conn.ExecuteAsync(sql, entry);
        }
        return count;
    }

    private static async Task InsertDailySummariesAsync(NpgsqlConnection conn, List<DailySummary> summaries)
    {
        const string sql = """
            INSERT INTO daily_summaries 
                (date, calories_consumed, calories_burned, calories_goal, total_carbs, total_fat, total_protein)
            VALUES 
                (@Date, @CaloriesConsumed, @CaloriesBurned, @CaloriesGoal, @TotalCarbs, @TotalFat, @TotalProtein)
            ON CONFLICT (date) DO UPDATE SET
                calories_consumed = EXCLUDED.calories_consumed,
                calories_goal     = EXCLUDED.calories_goal,
                total_carbs       = EXCLUDED.total_carbs,
                total_fat         = EXCLUDED.total_fat,
                total_protein     = EXCLUDED.total_protein,
                imported_at       = NOW()
            """;

        foreach (var summary in summaries)
            await conn.ExecuteAsync(sql, summary);
    }

    private static async Task RecalculateDailySummariesAsync(
        NpgsqlConnection conn,
        List<FoodEntry> foodEntries,
        List<ExerciseEntry> exerciseEntries)
    {
        // Get distinct dates from imported data
        var dates = foodEntries.Select(f => f.Date)
            .Union(exerciseEntries.Select(e => e.Date))
            .Distinct();

        const string upsertSql = """
            INSERT INTO daily_summaries 
                (date, calories_consumed, calories_burned, total_carbs, total_fat, total_protein)
            SELECT 
                date,
                COALESCE(SUM(calories), 0)      AS calories_consumed,
                0                               AS calories_burned,
                COALESCE(SUM(carbohydrates), 0) AS total_carbs,
                COALESCE(SUM(fat), 0)           AS total_fat,
                COALESCE(SUM(protein), 0)       AS total_protein
            FROM food_entries
            WHERE date = @Date
            GROUP BY date
            ON CONFLICT (date) DO UPDATE SET
                calories_consumed = EXCLUDED.calories_consumed,
                total_carbs       = EXCLUDED.total_carbs,
                total_fat         = EXCLUDED.total_fat,
                total_protein     = EXCLUDED.total_protein,
                imported_at       = NOW();
            
            UPDATE daily_summaries ds
            SET calories_burned = (
                SELECT COALESCE(SUM(calories_burned), 0)
                FROM exercise_entries
                WHERE date = @Date
            )
            WHERE ds.date = @Date;
            """;

        foreach (var date in dates)
            await conn.ExecuteAsync(upsertSql, new { Date = date });
    }

    private static async Task LogImportAsync(
        NpgsqlConnection conn, string fileName, string fileType,
        int rowsImported, List<string> warnings, bool success)
    {
        const string sql = """
            INSERT INTO import_log (file_name, file_type, rows_imported, warnings, success)
            VALUES (@FileName, @FileType, @RowsImported, @Warnings, @Success)
            """;

        await conn.ExecuteAsync(sql, new
        {
            FileName = fileName,
            FileType = fileType,
            RowsImported = rowsImported,
            Warnings = warnings.Any() ? string.Join("\n", warnings) : null,
            Success = success
        });
    }
}
