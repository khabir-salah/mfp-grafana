namespace MfpDashboard.Models.Entities;

public class UploadResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int FoodRowsImported { get; set; }
    public int ExerciseRowsImported { get; set; }
    public int WeightRowsImported { get; set; }
    public List<string> Warnings { get; set; } = new();
    public string? FileName { get; set; }
}