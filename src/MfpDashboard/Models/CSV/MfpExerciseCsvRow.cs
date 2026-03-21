namespace MfpDashboard.Models.CSV;


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