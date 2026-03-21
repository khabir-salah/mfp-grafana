namespace MfpDashboard.Services;


public interface ICsvParserService
{
    Task<ParsedCsvData> ParseAsync(Stream fileStream, string fileName);
}
