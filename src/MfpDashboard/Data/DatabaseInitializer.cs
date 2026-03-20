using Dapper;
using Npgsql;

namespace MfpDashboard.Data;

public class DatabaseInitializer
{
    private readonly string _connectionString;
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(IConfiguration configuration, ILogger<DatabaseInitializer> logger)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        _logger.LogInformation("Initializing database schema...");

        // Retry logic for Docker startup ordering
        var retries = 5;
        while (retries > 0)
        {
            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();
                await CreateTablesAsync(connection);
                _logger.LogInformation("Database initialized successfully.");
                return;
            }
            catch (Npgsql.PostgresException ex)
            {
                // This will tell you if it's "Invalid Password", "Database does not exist", etc.
                _logger.LogError("Postgres Error: {Message} (Code: {SqlState})", ex.Message, ex.SqlState);
                throw;
            }
            catch (Exception ex)
            {
                retries--;
                _logger.LogWarning("Database not ready, retrying in 3s... ({Retries} left). Error: {Error}",
                    retries, ex.Message);
                await Task.Delay(3000);
            }
        }

        throw new Exception("Could not connect to database after multiple retries.");
    }

    private static async Task CreateTablesAsync(NpgsqlConnection connection)
    {
        const string sql = """
            CREATE TABLE IF NOT EXISTS food_entries (
                id              BIGSERIAL PRIMARY KEY,
                date            DATE NOT NULL,
                meal_name       VARCHAR(100) NOT NULL DEFAULT '',
                food_name       VARCHAR(500) NOT NULL,
                calories        NUMERIC(10,2) NOT NULL DEFAULT 0,
                carbohydrates   NUMERIC(10,2) NOT NULL DEFAULT 0,
                fat             NUMERIC(10,2) NOT NULL DEFAULT 0,
                protein         NUMERIC(10,2) NOT NULL DEFAULT 0,
                cholesterol     NUMERIC(10,2) NOT NULL DEFAULT 0,
                sodium          NUMERIC(10,2) NOT NULL DEFAULT 0,
                sugar           NUMERIC(10,2) NOT NULL DEFAULT 0,
                fiber           NUMERIC(10,2) NOT NULL DEFAULT 0,
                imported_at     TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );

            CREATE TABLE IF NOT EXISTS exercise_entries (
                id              BIGSERIAL PRIMARY KEY,
                date            DATE NOT NULL,
                exercise_name   VARCHAR(500) NOT NULL,
                minutes         INTEGER NOT NULL DEFAULT 0,
                calories_burned NUMERIC(10,2) NOT NULL DEFAULT 0,
                sets            VARCHAR(50) NOT NULL DEFAULT '',
                reps            VARCHAR(50) NOT NULL DEFAULT '',
                weight          VARCHAR(50) NOT NULL DEFAULT '',
                imported_at     TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );

            CREATE TABLE IF NOT EXISTS weight_entries (
                id              BIGSERIAL PRIMARY KEY,
                date            DATE NOT NULL,
                weight          NUMERIC(8,2) NOT NULL,
                unit            VARCHAR(10) NOT NULL DEFAULT 'lbs',
                imported_at     TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                UNIQUE(date)
            );

            CREATE TABLE IF NOT EXISTS daily_summaries (
                id                  BIGSERIAL PRIMARY KEY,
                date                DATE NOT NULL UNIQUE,
                calories_consumed   NUMERIC(10,2) NOT NULL DEFAULT 0,
                calories_burned     NUMERIC(10,2) NOT NULL DEFAULT 0,
                calories_goal       NUMERIC(10,2) NOT NULL DEFAULT 0,
                total_carbs         NUMERIC(10,2) NOT NULL DEFAULT 0,
                total_fat           NUMERIC(10,2) NOT NULL DEFAULT 0,
                total_protein       NUMERIC(10,2) NOT NULL DEFAULT 0,
                weight              NUMERIC(8,2),
                imported_at         TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );

            CREATE TABLE IF NOT EXISTS import_log (
                id              BIGSERIAL PRIMARY KEY,
                file_name       VARCHAR(500) NOT NULL,
                file_type       VARCHAR(50) NOT NULL,
                rows_imported   INTEGER NOT NULL DEFAULT 0,
                warnings        TEXT,
                imported_at     TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                success         BOOLEAN NOT NULL DEFAULT TRUE
            );

            -- Indexes for Grafana time-range queries
            CREATE INDEX IF NOT EXISTS idx_food_entries_date ON food_entries(date);
            CREATE INDEX IF NOT EXISTS idx_exercise_entries_date ON exercise_entries(date);
            CREATE INDEX IF NOT EXISTS idx_weight_entries_date ON weight_entries(date);
            CREATE INDEX IF NOT EXISTS idx_daily_summaries_date ON daily_summaries(date);
            """;

        await connection.ExecuteAsync(sql);
    }

    public string ConnectionString => _connectionString;
}
