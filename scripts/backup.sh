#!/usr/bin/env bash
# Backup PostgreSQL data to a timestamped file.
set -euo pipefail
set -a; source .env; set +a

BACKUP_DIR="./backups"
mkdir -p "$BACKUP_DIR"
TIMESTAMP=$(date +%Y%m%d_%H%M%S)
FILE="$BACKUP_DIR/mfp_data_${TIMESTAMP}.sql.gz"

echo "Creating backup: $FILE"
docker exec mfp-postgres pg_dump \
  -U "${POSTGRES_USER:-mfp_user}" \
  -d "${POSTGRES_DB:-mfp_data}" \
  --no-owner \
  --no-acl \
  | gzip > "$FILE"

echo "Backup complete: $FILE ($(du -sh "$FILE" | cut -f1))"

# Keep only the last 10 backups
ls -t "$BACKUP_DIR"/*.sql.gz 2>/dev/null | tail -n +11 | xargs rm -f 2>/dev/null || true
echo "Old backups pruned. Total kept: $(ls "$BACKUP_DIR"/*.sql.gz 2>/dev/null | wc -l)"
