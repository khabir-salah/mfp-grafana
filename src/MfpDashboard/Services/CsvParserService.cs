using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using MfpDashboard.Models;

namespace MfpDashboard.Services;

public interface ICsvParserService
{
    Task<ParsedCsvData> ParseAsync(Stream fileStream, string fileName);
}

public class ParsedCsvData
{
    public List<FoodEntry> FoodEntries { get; set; } = new();
    public List<ExerciseEntry> ExerciseEntries { get; set; } = new();
    public List<WeightEntry> WeightEntries { get; set; } = new();
    public List<DailySummary> DailySummaries { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public CsvFileType DetectedType { get; set; }
}

public enum CsvFileType
{
    Food,
    Exercise,
    Measurements,
    NutritionSummary,
    Unknown
}

public class CsvParserService : ICsvParserService
{
    private readonly ILogger<CsvParserService> _logger;

    public CsvParserService(ILogger<CsvParserService> logger)
    {
        _logger = logger;
    }

    public async Task<ParsedCsvData> ParseAsync(Stream fileStream, string fileName)
    {
        var result = new ParsedCsvData();

        using var reader = new StreamReader(fileStream);
        var firstLine = await reader.ReadLineAsync() ?? string.Empty;

        // Reset stream
        fileStream.Seek(0, SeekOrigin.Begin);

        result.DetectedType = DetectFileType(firstLine, fileName);
        _logger.LogInformation("Detected CSV type: {Type} for file: {FileName}", result.DetectedType, fileName);

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HeaderValidated = null,
            MissingFieldFound = null,
            BadDataFound = context =>
            {
                result.Warnings.Add($"Bad data at row {context.Context.Parser.Row}: {context.RawRecord}");
            }
        };

        try
        {
            switch (result.DetectedType)
            {
                case CsvFileType.Food:
                    result.FoodEntries = await ParseFoodCsvAsync(fileStream, config, result.Warnings);
                    break;

                case CsvFileType.Exercise:
                    result.ExerciseEntries = await ParseExerciseCsvAsync(fileStream, config, result.Warnings);
                    break;

                case CsvFileType.Measurements:
                    result.WeightEntries = await ParseMeasurementsCsvAsync(fileStream, config, result.Warnings);
                    break;

                case CsvFileType.NutritionSummary:
                    result.DailySummaries = await ParseNutritionSummaryCsvAsync(fileStream, config, result.Warnings);
                    break;

                default:
                    // Try to auto-detect and parse as food
                    result.Warnings.Add("Could not confidently detect CSV type. Attempting food log parse.");
                    result.FoodEntries = await ParseFoodCsvAsync(fileStream, config, result.Warnings);
                    result.DetectedType = CsvFileType.Food;
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing CSV file {FileName}", fileName);
            result.Warnings.Add($"Parse error: {ex.Message}");
        }

        return result;
    }

    private static CsvFileType DetectFileType(string header, string fileName)
    {
        var h = header.ToLowerInvariant();
        var f = fileName.ToLowerInvariant();

        if (h.Contains("food name") || h.Contains("meal"))
            return CsvFileType.Food;
        if (h.Contains("exercise name") || h.Contains("calories burned") || f.Contains("exercise"))
            return CsvFileType.Exercise;
        if ((h.Contains("weight") && !h.Contains("exercise")) || f.Contains("measurement"))
            return CsvFileType.Measurements;
        if (h.Contains("calorie goal") || h.Contains("calories remaining") || f.Contains("nutrition"))
            return CsvFileType.NutritionSummary;

        return CsvFileType.Unknown;
    }

    private static async Task<List<FoodEntry>> ParseFoodCsvAsync(
        Stream stream, CsvConfiguration config, List<string> warnings)
    {
        stream.Seek(0, SeekOrigin.Begin);
        var entries = new List<FoodEntry>();

        using var reader = new StreamReader(stream, leaveOpen: true);
        using var csv = new CsvReader(reader, config);
        csv.Context.RegisterClassMap<MfpFoodCsvRowMap>();

        await foreach (var row in csv.GetRecordsAsync<MfpFoodCsvRow>())
        {
            // Skip summary/total rows that MFP sometimes appends
            if (string.IsNullOrWhiteSpace(row.Date) ||
                row.FoodName.StartsWith("Totals", StringComparison.OrdinalIgnoreCase))
                continue;

            if (!TryParseDate(row.Date, out var date))
            {
                warnings.Add($"Could not parse date '{row.Date}', skipping row.");
                continue;
            }

            entries.Add(new FoodEntry
            {
                Date = date,
                MealName = row.Meal?.Trim() ?? string.Empty,
                FoodName = row.FoodName?.Trim() ?? string.Empty,
                Calories = ParseDecimal(row.Calories),
                Fat = ParseDecimal(row.Fat),
                Carbohydrates = ParseDecimal(row.Carbohydrates),
                Protein = ParseDecimal(row.Protein),
                Cholesterol = ParseDecimal(row.Cholesterol),
                Sodium = ParseDecimal(row.Sodium),
                Sugar = ParseDecimal(row.Sugar),
                Fiber = ParseDecimal(row.Fiber),
            });
        }

        return entries;
    }

    private static async Task<List<ExerciseEntry>> ParseExerciseCsvAsync(
        Stream stream, CsvConfiguration config, List<string> warnings)
    {
        stream.Seek(0, SeekOrigin.Begin);
        var entries = new List<ExerciseEntry>();

        using var reader = new StreamReader(stream, leaveOpen: true);
        using var csv = new CsvReader(reader, config);

        await foreach (var row in csv.GetRecordsAsync<MfpExerciseCsvRow>())
        {
            if (string.IsNullOrWhiteSpace(row.Date) || string.IsNullOrWhiteSpace(row.ExerciseName))
                continue;

            if (!TryParseDate(row.Date, out var date))
            {
                warnings.Add($"Could not parse date '{row.Date}', skipping row.");
                continue;
            }

            entries.Add(new ExerciseEntry
            {
                Date = date,
                ExerciseName = row.ExerciseName.Trim(),
                Minutes = (int)ParseDecimal(row.Minutes),
                CaloriesBurned = ParseDecimal(row.CaloriesBurned),
                Sets = row.Sets?.Trim() ?? string.Empty,
                Reps = row.Reps?.Trim() ?? string.Empty,
                Weight = row.Weight?.Trim() ?? string.Empty,
            });
        }

        return entries;
    }

    private static async Task<List<WeightEntry>> ParseMeasurementsCsvAsync(
        Stream stream, CsvConfiguration config, List<string> warnings)
    {
        stream.Seek(0, SeekOrigin.Begin);
        var entries = new List<WeightEntry>();

        using var reader = new StreamReader(stream, leaveOpen: true);
        using var csv = new CsvReader(reader, config);

        await foreach (var row in csv.GetRecordsAsync<MfpMeasurementCsvRow>())
        {
            if (string.IsNullOrWhiteSpace(row.Date))
                continue;

            if (!TryParseDate(row.Date, out var date))
            {
                warnings.Add($"Could not parse date '{row.Date}', skipping row.");
                continue;
            }

            var weight = ParseDecimal(row.Weight);
            if (weight <= 0) continue;

            entries.Add(new WeightEntry
            {
                Date = date,
                Weight = weight,
                Unit = string.IsNullOrWhiteSpace(row.Unit) ? "lbs" : row.Unit.Trim(),
            });
        }

        return entries;
    }

    private static async Task<List<DailySummary>> ParseNutritionSummaryCsvAsync(
        Stream stream, CsvConfiguration config, List<string> warnings)
    {
        stream.Seek(0, SeekOrigin.Begin);
        var entries = new List<DailySummary>();

        using var reader = new StreamReader(stream, leaveOpen: true);
        using var csv = new CsvReader(reader, config);

        await foreach (var row in csv.GetRecordsAsync<MfpNutritionSummaryCsvRow>())
        {
            if (string.IsNullOrWhiteSpace(row.Date))
                continue;

            if (!TryParseDate(row.Date, out var date))
            {
                warnings.Add($"Could not parse date '{row.Date}', skipping row.");
                continue;
            }

            entries.Add(new DailySummary
            {
                Date = date,
                CaloriesConsumed = ParseDecimal(row.Calories),
                CaloriesGoal = ParseDecimal(row.CalorieGoal),
                TotalCarbs = ParseDecimal(row.Carbohydrates),
                TotalFat = ParseDecimal(row.Fat),
                TotalProtein = ParseDecimal(row.Protein),
            });
        }

        return entries;
    }

    private static bool TryParseDate(string input, out DateOnly date)
    {
        date = default;
        if (string.IsNullOrWhiteSpace(input)) return false;

        string[] formats = {
            "yyyy-MM-dd", "MM/dd/yyyy", "dd/MM/yyyy",
            "M/d/yyyy", "d/M/yyyy", "yyyy/MM/dd",
            "MM-dd-yyyy", "dd-MM-yyyy"
        };

        foreach (var fmt in formats)
        {
            if (DateOnly.TryParseExact(input.Trim(), fmt,
                CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                return true;
        }

        return false;
    }

    private static decimal ParseDecimal(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return 0;
        var cleaned = value.Replace(",", "").Trim();
        return decimal.TryParse(cleaned, NumberStyles.Any,
            CultureInfo.InvariantCulture, out var result) ? result : 0;
    }
}
