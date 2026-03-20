using Microsoft.AspNetCore.Mvc;
using MfpDashboard.Models;
using MfpDashboard.Services;

namespace MfpDashboard.Controllers;

public class HomeController : Controller
{
    private readonly IStatsService _stats;
    private readonly ILogger<HomeController> _logger;
    private readonly IConfiguration _config;

    public HomeController(IStatsService stats, ILogger<HomeController> logger, IConfiguration config)
    {
        _stats = stats;
        _logger = logger;
        _config = config;
    }

    public async Task<IActionResult> Index()
    {
        var stats = await _stats.GetStatsAsync();
        var imports = await _stats.GetRecentImportsAsync();

        ViewBag.GrafanaUrl = _config["GrafanaSettings:BaseUrl"] ?? "http://localhost:3000";
        ViewBag.Stats = stats;
        ViewBag.RecentImports = imports;

        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View();
    }
}
