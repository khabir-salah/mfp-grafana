-- This script runs once when the PostgreSQL container is first created.
-- The application's DatabaseInitializer will handle the actual table creation
-- on startup, but we set up any DB-level configuration here.

-- Ensure the database uses UTC
ALTER DATABASE mfp_data SET timezone TO 'UTC';

-- Optional: Create a read-only user for Grafana (more secure than using the app user)
-- Grafana only needs SELECT access to query dashboards.
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT FROM pg_catalog.pg_roles WHERE rolname = 'grafana_reader'
    ) THEN
        CREATE ROLE grafana_reader WITH LOGIN PASSWORD 'grafana_readonly_CHANGE_ME';
    END IF;
END
$$;

-- Grant read access (tables created by the app will need this run after first startup)
-- Run manually after first docker-compose up:
--   docker exec mfp-postgres psql -U mfp_user -d mfp_data -c "
--     GRANT SELECT ON ALL TABLES IN SCHEMA public TO grafana_reader;
--     ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT SELECT ON TABLES TO grafana_reader;
--   "
