using Microsoft.AspNetCore.Mvc;
using MfpDashboard.Services;
using MfpDashboard.Models.Entities;

namespace MfpDashboard.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UploadController : ControllerBase
{
    private readonly ICsvParserService _parser;
    private readonly IIngestionService _ingestion;
    private readonly IConfiguration _config;
    private readonly ILogger<UploadController> _logger;

    public UploadController(
        ICsvParserService parser,
        IIngestionService ingestion,
        IConfiguration config,
        ILogger<UploadController> logger)
    {
        _parser = parser;
        _ingestion = ingestion;
        _config = config;
        _logger = logger;
    }

    [HttpPost]
    [RequestSizeLimit(50_000_000)] // 50MB max
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new UploadResult { Success = false, Message = "No file uploaded." });

        var maxSizeMb = _config.GetValue<int>("UploadSettings:MaxFileSizeMb", 10);
        if (file.Length > maxSizeMb * 1024 * 1024)
            return BadRequest(new UploadResult
            {
                Success = false,
                Message = $"File too large. Maximum size is {maxSizeMb}MB."
            });

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (ext != ".csv")
            return BadRequest(new UploadResult
            {
                Success = false,
                Message = "Only CSV files are accepted."
            });

        _logger.LogInformation("Received upload: {FileName} ({Size} bytes)", file.FileName, file.Length);

        try
        {
            await using var stream = file.OpenReadStream();
            var parsed = await _parser.ParseAsync(stream, file.FileName);

            if (parsed.FoodEntries.Count == 0 &&
                parsed.ExerciseEntries.Count == 0 &&
                parsed.WeightEntries.Count == 0 &&
                parsed.DailySummaries.Count == 0)
            {
                return BadRequest(new UploadResult
                {
                    Success = false,
                    Message = "No valid data rows found in the CSV file. Please check the format.",
                    Warnings = parsed.Warnings
                });
            }

            var result = await _ingestion.IngestAsync(parsed, file.FileName);
            return result.Success ? Ok(result) : StatusCode(500, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error processing {FileName}", file.FileName);
            return StatusCode(500, new UploadResult
            {
                Success = false,
                Message = $"Unexpected error: {ex.Message}"
            });
        }
    }

    [HttpGet("status")]
    public IActionResult Status() => Ok(new { status = "ok", timestamp = DateTime.UtcNow });
}
