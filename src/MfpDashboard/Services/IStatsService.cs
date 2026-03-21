using MfpDashboard.Models.Entities;

namespace MfpDashboard.Services;


public interface IStatsService
{
    Task<DashboardStats> GetStatsAsync();
    Task<List<ImportLogEntry>> GetRecentImportsAsync(int count = 10);
}