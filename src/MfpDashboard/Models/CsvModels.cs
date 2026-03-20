using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;

namespace MfpDashboard.Models;

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

/// <summary>
/// Represents a row in the MFP exercise CSV export.
/// </summary>
public class MfpExerciseCsvRow
{
    [Name("Date")]
    public string Date { get; set; } = string.Empty;

    [Name("Exercise Name")]
    public string ExerciseName { get; set; } = string.Empty;

    [Name("Minutes")]
    public string Minutes { get; set; } = "0";

    [Name("Calories Burned")]
    public string CaloriesBurned { get; set; } = "0";

    [Name("Sets")]
    public string Sets { get; set; } = string.Empty;

    [Name("Reps/Duration")]
    public string Reps { get; set; } = string.Empty;

    [Name("Weight/Distance")]
    public string Weight { get; set; } = string.Empty;
}

/// <summary>
/// Represents a row in the MFP measurements/weight CSV export.
/// </summary>
public class MfpMeasurementCsvRow
{
    [Name("Date")]
    public string Date { get; set; } = string.Empty;

    [Name("Weight")]
    public string Weight { get; set; } = "0";

    [Name("Unit")]
    public string Unit { get; set; } = "lbs";
}

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

public sealed class MfpFoodCsvRowMap : ClassMap<MfpFoodCsvRow>
{
    public MfpFoodCsvRowMap()
    {
        AutoMap(System.Globalization.CultureInfo.InvariantCulture);
        Map(m => m.Fat).Name("Fat (g)", "Fat");
        Map(m => m.Cholesterol).Name("Cholesterol (mg)", "Cholesterol");
        Map(m => m.Sodium).Name("Sodium (mg)", "Sodium");
        Map(m => m.Carbohydrates).Name("Carbohydrates (g)", "Carbohydrates");
        Map(m => m.Fiber).Name("Fiber (g)", "Fiber");
        Map(m => m.Sugar).Name("Sugar (g)", "Sugar");
        Map(m => m.Protein).Name("Protein (g)", "Protein");
    }
}
