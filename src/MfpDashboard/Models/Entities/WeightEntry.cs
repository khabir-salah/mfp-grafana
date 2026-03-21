namespace MfpDashboard.Models.Entities;

public class WeightEntry
{
    public long Id { get; set; }
    public DateOnly Date { get; set; }
    public decimal Weight { get; set; }
    public string Unit { get; set; } = "lbs";
    public DateTime ImportedAt { get; set; } = DateTime.UtcNow;
}