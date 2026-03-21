namespace MfpDashboard.Services;


public class ImportLogEntry
{
    public long Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public int RowsImported { get; set; }
    public bool Success { get; set; }
    public DateTime ImportedAt { get; set; }
    public string? Warnings { get; set; }
}