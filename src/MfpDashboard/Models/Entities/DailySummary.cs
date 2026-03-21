namespace MfpDashboard.Models.Entities;


public class DailySummary
{
    public long Id { get; set; }
    public DateOnly Date { get; set; }
    public decimal CaloriesConsumed { get; set; }
    public decimal CaloriesBurned { get; set; }
    public decimal CaloriesGoal { get; set; }
    public decimal TotalCarbs { get; set; }
    public decimal TotalFat { get; set; }
    public decimal TotalProtein { get; set; }
    public decimal? Weight { get; set; }
    public DateTime ImportedAt { get; set; } = DateTime.UtcNow;
}