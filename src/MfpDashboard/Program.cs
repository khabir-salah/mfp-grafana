
using Dapper;
using MfpDashboard.Data;
using MfpDashboard.Services;
using Serilog;

// Register DateOnly support for Dapper — must run before any DB calls
SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/mfp-dashboard-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    Log.Information("Starting MFP Dashboard application");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();

    builder.Services.AddControllersWithViews();
    builder.Services.AddSingleton<DatabaseInitializer>();
    builder.Services.AddScoped<ICsvParserService, CsvParserService>();
    builder.Services.AddScoped<IIngestionService, IngestionService>();
    builder.Services.AddScoped<IStatsService, StatsService>();

    var app = builder.Build();

    // Initialize DB on startup
    using (var scope = app.Services.CreateScope())
    {
        var dbInit = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
        await dbInit.InitializeAsync();
    }

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }

    app.UseStaticFiles();
    app.UseRouting();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
