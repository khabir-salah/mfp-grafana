namespace MfpDashboard.Models.CSV;


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