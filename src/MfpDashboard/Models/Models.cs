namespace MfpDashboard.Models;

public class FoodEntry
{
    public long Id { get; set; }
    public DateOnly Date { get; set; }
    public string MealName { get; set; } = string.Empty;
    public string FoodName { get; set; } = string.Empty;
    public decimal Calories { get; set; }
    public decimal Carbohydrates { get; set; }
    public decimal Fat { get; set; }
    public decimal Protein { get; set; }
    public decimal Cholesterol { get; set; }
    public decimal Sodium { get; set; }
    public decimal Sugar { get; set; }
    public decimal Fiber { get; set; }
    public DateTime ImportedAt { get; set; } = DateTime.UtcNow;
}

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

public class WeightEntry
{
    public long Id { get; set; }
    public DateOnly Date { get; set; }
    public decimal Weight { get; set; }
    public string Unit { get; set; } = "lbs";
    public DateTime ImportedAt { get; set; } = DateTime.UtcNow;
}

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
