namespace MfpDashboard.Models.Entities;


public class ExerciseEntry
{
    public long Id { get; set; }
    public DateOnly Date { get; set; }
    public string ExerciseName { get; set; } = string.Empty;
    public int Minutes { get; set; }
    public decimal CaloriesBurned { get; set; }
    public string Sets { get; set; } = string.Empty;
    public string Reps { get; set; } = string.Empty;
    public string Weight { get; set; } = string.Empty;
    public DateTime ImportedAt { get; set; } = DateTime.UtcNow;
}