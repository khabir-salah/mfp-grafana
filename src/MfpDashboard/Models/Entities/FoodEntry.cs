

namespace MfpDashboard.Models.Entities;

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