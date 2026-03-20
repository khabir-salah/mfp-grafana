#!/usr/bin/env bash
# =============================================================================
# MFP Dashboard — Linux Setup Script
# Tested on Ubuntu 22.04 / Debian 12
# Run as a regular user with sudo access.
# =============================================================================
set -euo pipefail

RED='\033[0;31m'; GREEN='\033[0;32m'; YELLOW='\033[1;33m'; CYAN='\033[0;36m'; NC='\033[0m'
info()    { echo -e "${CYAN}[INFO]${NC}  $*"; }
success() { echo -e "${GREEN}[OK]${NC}    $*"; }
warn()    { echo -e "${YELLOW}[WARN]${NC}  $*"; }
error()   { echo -e "${RED}[ERROR]${NC} $*"; exit 1; }

# ── Require .env ──────────────────────────────────────────────────────────────
if [ ! -f ".env" ]; then
    warn ".env not found — copying from .env.example"
    cp .env.example .env
    error "Please edit .env with your passwords and settings, then re-run this script."
fi

# Source .env for validation
set -a; source .env; set +a

# Validate required vars
: "${POSTGRES_PASSWORD:?Set POSTGRES_PASSWORD in .env}"
: "${GF_ADMIN_PASSWORD:?Set GF_ADMIN_PASSWORD in .env}"

# ── Docker check ──────────────────────────────────────────────────────────────
info "Checking Docker..."
if ! command -v docker &>/dev/null; then
    info "Installing Docker..."
    curl -fsSL https://get.docker.com | sh
    sudo usermod -aG docker "$USER"
    warn "Added $USER to docker group. You may need to log out and back in."
fi

if ! docker info &>/dev/null; then
    sudo systemctl start docker
    sudo systemctl enable docker
fi
success "Docker ready"

# ── Docker Compose check ──────────────────────────────────────────────────────
if ! docker compose version &>/dev/null && ! docker-compose version &>/dev/null; then
    info "Installing Docker Compose plugin..."
    sudo apt-get update -qq
    sudo apt-get install -y docker-compose-plugin
fi
success "Docker Compose ready"

# ── Create ssl placeholder directory ─────────────────────────────────────────
mkdir -p nginx/ssl

# ── Build & Start ─────────────────────────────────────────────────────────────
info "Building Docker images..."
docker compose build --no-cache

info "Starting services..."
docker compose up -d

# ── Wait for health ───────────────────────────────────────────────────────────
info "Waiting for services to become healthy..."
ATTEMPTS=0
MAX_ATTEMPTS=30

until docker compose ps | grep -q "healthy" || [ $ATTEMPTS -ge $MAX_ATTEMPTS ]; do
    sleep 5
    ATTEMPTS=$((ATTEMPTS + 1))
    echo -n "."
done
echo ""

# ── Grant Grafana read access ─────────────────────────────────────────────────
info "Granting Grafana read-only access to PostgreSQL..."
sleep 5   # Give the webapp a moment to create tables

docker exec mfp-postgres psql -U "${POSTGRES_USER:-mfp_user}" -d "${POSTGRES_DB:-mfp_data}" -c "
    DO \$\$
    BEGIN
        IF NOT EXISTS (SELECT FROM pg_roles WHERE rolname = 'grafana_reader') THEN
            CREATE ROLE grafana_reader WITH LOGIN PASSWORD 'grafana_readonly_CHANGE_ME';
        END IF;
    END
    \$\$;
    GRANT SELECT ON ALL TABLES IN SCHEMA public TO grafana_reader;
    ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT SELECT ON TABLES TO grafana_reader;
" 2>/dev/null || warn "Could not set up grafana_reader — tables may not exist yet. Run scripts/grant-grafana.sh after first upload."

# ── Done ──────────────────────────────────────────────────────────────────────
echo ""
echo -e "${GREEN}══════════════════════════════════════════════════════${NC}"
success "MFP Dashboard is running!"
echo ""
echo -e "  Web App:  ${CYAN}http://localhost/${NC}"
echo -e "  Grafana:  ${CYAN}http://localhost/grafana/${NC}"
echo -e "  Grafana login: ${YELLOW}${GF_ADMIN_USER:-admin} / ${GF_ADMIN_PASSWORD}${NC}"
echo ""
echo -e "  View logs:   ${CYAN}docker compose logs -f${NC}"
echo -e "  Stop:        ${CYAN}docker compose down${NC}"
echo -e "${GREEN}══════════════════════════════════════════════════════${NC}"
