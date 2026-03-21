using MfpDashboard.Models.Entities;

namespace MfpDashboard.Services;


public interface IIngestionService
{
    Task<UploadResult> IngestAsync(ParsedCsvData data, string fileName);
}