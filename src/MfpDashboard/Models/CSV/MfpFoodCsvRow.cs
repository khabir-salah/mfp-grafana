namespace MfpDashboard.Models.CSV;


/// <summary>
/// Represents a row in the MFP food CSV export.
/// MFP CSV format varies slightly by region/version; we handle both.
/// </summary>
public class MfpFoodCsvRow
{
    [Name("Date")]
    public string Date { get; set; } = string.Empty;
 
    [Name("Meal")]
    public string Meal { get; set; } = string.Empty;
 
    [Name("Food Name")]
    public string FoodName { get; set; } = string.Empty;
 
    [Name("Calories")]
    public string Calories { get; set; } = "0";
 
    [Name("Fat (g)")]
    public string Fat { get; set; } = "0";
 
    [Name("Saturated Fat")]
    public string SaturatedFat { get; set; } = "0";
 
    [Name("Polyunsaturated Fat")]
    public string PolyunsaturatedFat { get; set; } = "0";
 
    [Name("Monounsaturated Fat")]
    public string MonounsaturatedFat { get; set; } = "0";
 
    [Name("Cholesterol (mg)")]
    public string Cholesterol { get; set; } = "0";
 
    [Name("Sodium (mg)")]
    public string Sodium { get; set; } = "0";
 
    [Name("Carbohydrates (g)")]
    public string Carbohydrates { get; set; } = "0";
 
    [Name("Fiber (g)")]
    public string Fiber { get; set; } = "0";
 
    [Name("Sugar (g)")]
    public string Sugar { get; set; } = "0";
 
    [Name("Protein (g)")]
    public string Protein { get; set; } = "0";
}