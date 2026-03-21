namespace MfpDashboard.Services;


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