

using Dapper;
using MfpDashboard.Data;
using MfpDashboard.Models;
using MfpDashboard.Models.Entities;
using Npgsql;

namespace MfpDashboard.Services;


public class StatsService : IStatsService
{
    private readonly string _connectionString;

    public StatsService(DatabaseInitializer dbInit)
    {
        _connectionString = dbInit.ConnectionString;
    }

    public async Task<DashboardStats> GetStatsAsync()
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        const string sql = """
            SELECT
                COUNT(DISTINCT date)                AS total_days_tracked,
                COALESCE(AVG(calories_consumed), 0) AS avg_daily_calories,
                COALESCE(AVG(total_protein), 0)     AS avg_protein,
                COALESCE(AVG(total_carbs), 0)       AS avg_carbs,
                COALESCE(AVG(total_fat), 0)         AS avg_fat,
                MIN(date)                           AS earliest_date,
                MAX(date)                           AS latest_date
            FROM daily_summaries;

            SELECT COUNT(*) FROM food_entries;
            SELECT COUNT(*) FROM exercise_entries;

            SELECT weight FROM weight_entries ORDER BY date DESC LIMIT 1;
            SELECT weight FROM weight_entries ORDER BY date ASC  LIMIT 1;
            """;

        await using var multi = await conn.QueryMultipleAsync(sql);

        var summary = await multi.ReadSingleOrDefaultAsync<dynamic>();
        var foodCount = await multi.ReadSingleAsync<int>();
        var exerciseCount = await multi.ReadSingleAsync<int>();
        var latestWeight = await multi.ReadFirstOrDefaultAsync<decimal?>();
        var earliestWeight = await multi.ReadFirstOrDefaultAsync<decimal?>();

        return new DashboardStats
        {
            TotalDaysTracked = (int)(summary?.total_days_tracked ?? 0),
            AverageDailyCalories = (decimal)(summary?.avg_daily_calories ?? 0),
            AverageProtein = (decimal)(summary?.avg_protein ?? 0),
            AverageCarbs = (decimal)(summary?.avg_carbs ?? 0),
            AverageFat = (decimal)(summary?.avg_fat ?? 0),
            TotalFoodEntries = foodCount,
            TotalExerciseEntries = exerciseCount,
            CurrentWeight = latestWeight,
            WeightChange = (latestWeight.HasValue && earliestWeight.HasValue)
                ? latestWeight.Value - earliestWeight.Value
                : null,
            EarliestDate = summary?.earliest_date is DateTime ed
                ? DateOnly.FromDateTime(ed) : null,
            LatestDate = summary?.latest_date is DateTime ld
                ? DateOnly.FromDateTime(ld) : null,
        };
    }

    public async Task<List<ImportLogEntry>> GetRecentImportsAsync(int count = 10)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        var entries = await conn.QueryAsync<ImportLogEntry>(
            """
            SELECT id, file_name, file_type, rows_imported, success, imported_at, warnings
            FROM import_log
            ORDER BY imported_at DESC
            LIMIT @Count
            """,
            new { Count = count });

        return entries.ToList();
    }
}