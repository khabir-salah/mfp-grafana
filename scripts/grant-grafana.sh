#!/usr/bin/env bash
# Grant grafana_reader SELECT access to all current and future tables.
# Run this after your first CSV upload so the tables exist.
set -euo pipefail
set -a; source .env; set +a

docker exec mfp-postgres psql \
  -U "${POSTGRES_USER:-mfp_user}" \
  -d "${POSTGRES_DB:-mfp_data}" \
  -c "
    DO \$\$
    BEGIN
        IF NOT EXISTS (SELECT FROM pg_roles WHERE rolname = 'grafana_reader') THEN
            CREATE ROLE grafana_reader WITH LOGIN PASSWORD 'grafana_readonly_CHANGE_ME';
        END IF;
    END
    \$\$;
    GRANT SELECT ON ALL TABLES IN SCHEMA public TO grafana_reader;
    ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT SELECT ON TABLES TO grafana_reader;
  "

echo "Done — grafana_reader has SELECT on all tables."
