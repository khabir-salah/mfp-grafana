namespace MfpDashboard.Models.Entities;

public class DashboardStats
{
    public int TotalDaysTracked { get; set; }
    public decimal AverageDailyCalories { get; set; }
    public decimal AverageProtein { get; set; }
    public decimal AverageCarbs { get; set; }
    public decimal AverageFat { get; set; }
    public decimal? CurrentWeight { get; set; }
    public decimal? WeightChange { get; set; }
    public int TotalFoodEntries { get; set; }
    public int TotalExerciseEntries { get; set; }
    public DateOnly? EarliestDate { get; set; }
    public DateOnly? LatestDate { get; set; }
}