namespace MfpDashboard.Models.CSV;


/// <summary>
/// MFP nutrition summary row (daily totals).
/// </summary>
public class MfpNutritionSummaryCsvRow
{
    [Name("Date")]
    public string Date { get; set; } = string.Empty;
 
    [Name("Calories")]
    public string Calories { get; set; } = "0";
 
    [Name("Fat (g)")]
    public string Fat { get; set; } = "0";
 
    [Name("Carbohydrates (g)")]
    public string Carbohydrates { get; set; } = "0";
 
    [Name("Protein (g)")]
    public string Protein { get; set; } = "0";
 
    [Name("Calorie Goal")]
    public string CalorieGoal { get; set; } = "0";
 
    [Name("Calories Remaining")]
    public string CaloriesRemaining { get; set; } = "0";
}