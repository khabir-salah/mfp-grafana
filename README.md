# MFP Dashboard

A production-ready web application for visualizing MyFitnessPal (MFP) CSV exports through Grafana dashboards. Built with ASP.NET Core 8, PostgreSQL, Grafana, and Nginx — all containerized with Docker.

```
┌──────────────┐    CSV    ┌─────────────────┐    SQL     ┌────────────┐
│ MyFitnessPal │ ────────▶ │  ASP.NET Core   │ ─────────▶ │ PostgreSQL │
│   Export     │           │   Web App       │            │            │
└──────────────┘           └─────────────────┘            └─────┬──────┘
                                    │                            │
                           ┌────────▼────────┐     Query  ┌─────▼──────┐
                           │      Nginx      │ ◀───────── │  Grafana   │
                           │  Reverse Proxy  │            │ Dashboards │
                           └─────────────────┘            └────────────┘
```

---

## Table of Contents

1. [Prerequisites](#1-prerequisites)
2. [Project Structure](#2-project-structure)
3. [Quick Start](#3-quick-start)
4. [Configuration Reference](#4-configuration-reference)
5. [Exporting Data from MyFitnessPal](#5-exporting-data-from-myfitnesspal)
6. [Database Schema](#6-database-schema)
7. [Grafana Dashboards](#7-grafana-dashboards)
8. [HTTPS / SSL Setup](#8-https--ssl-setup)
9. [Useful Commands](#9-useful-commands)
10. [Troubleshooting](#10-troubleshooting)
11. [Development Setup](#11-development-setup)
12. [Security Checklist](#12-security-checklist)

---

## 1. Prerequisites

| Requirement | Version | Notes |
|---|---|---|
| Linux (Ubuntu/Debian) | 22.04+ / 12+ | Tested on Ubuntu 22.04 LTS |
| Docker Engine | 24+ | Installed automatically by setup script |
| Docker Compose | v2+ | Plugin version, installed automatically |
| Open ports | 80, 443 | For Nginx reverse proxy |
| RAM | ≥ 1 GB | 2 GB recommended for all services |
| Disk | ≥ 5 GB | For Docker images, DB, and logs |

---

## 2. Project Structure

```
mfp-grafana/
├── src/
│   └── MfpDashboard/               # ASP.NET Core 8 web application
│       ├── Controllers/
│       │   ├── HomeController.cs   # Dashboard overview page
│       │   └── UploadController.cs # CSV upload API endpoint
│       ├── Data/
│       │   └── DatabaseInitializer.cs  # Auto-creates PostgreSQL tables
│       ├── Models/
│       │   ├── Models.cs           # Domain models (FoodEntry, etc.)
│       │   └── CsvModels.cs        # CsvHelper row mappings
│       ├── Services/
│       │   ├── CsvParserService.cs # Detects & parses MFP CSV formats
│       │   ├── IngestionService.cs # Writes parsed data to PostgreSQL
│       │   └── StatsService.cs     # Queries summary stats for UI
│       ├── Views/
│       │   ├── Home/Index.cshtml   # Main dashboard page
│       │   └── Shared/_Layout.cshtml
│       ├── wwwroot/
│       │   ├── css/site.css        # Dark industrial UI theme
│       │   └── js/site.js          # Drag-and-drop upload logic
│       ├── appsettings.json
│       └── MfpDashboard.csproj
├── grafana/
│   ├── provisioning/
│   │   ├── datasources/postgres.yaml   # Auto-provisions PostgreSQL datasource
│   │   └── dashboards/dashboards.yaml  # Auto-loads dashboard JSON files
│   └── dashboards/
│       ├── mfp-overview.json       # Calories consumed vs goal
│       ├── mfp-macros.json         # Protein / carbs / fat breakdown
│       ├── mfp-exercise.json       # Exercise sessions and calories burned
│       └── mfp-weight.json         # Weight trend with moving average
├── nginx/
│   ├── nginx.conf                  # Main Nginx config (gzip, headers)
│   ├── conf.d/default.conf         # Virtual host (HTTP + HTTPS template)
│   └── ssl/                        # Place SSL certs here (see Section 8)
├── scripts/
│   ├── setup.sh                    # One-shot Linux setup + Docker start
│   ├── grant-grafana.sh            # Grants Grafana read-only DB access
│   └── backup.sh                   # PostgreSQL backup to gzip file
├── Dockerfile                      # Multi-stage ASP.NET Core image
├── docker-compose.yml              # All services: postgres, webapp, grafana, nginx
├── .env                            # Your local secrets (git-ignored)
├── .env.example                    # Template — commit this, not .env
└── README.md
```

---

## 3. Quick Start

### Step 1 — Clone or copy the project

```bash
git clone https://github.com/yourname/mfp-grafana.git
cd mfp-grafana
```

### Step 2 — Configure environment

```bash
cp .env.example .env
nano .env          # or: vim .env
```

Set these values — **do not leave defaults in production**:

```env
POSTGRES_PASSWORD=your_strong_password_here
GF_ADMIN_PASSWORD=your_grafana_password_here
SERVER_NAME=localhost          # or your domain: dashboard.example.com
GRAFANA_URL=http://localhost/grafana
```

### Step 3 — Run the setup script

```bash
chmod +x scripts/setup.sh
./scripts/setup.sh
```

This script will:
- Install Docker and Docker Compose if missing
- Build the ASP.NET Core image
- Start all four containers (postgres, webapp, grafana, nginx)
- Wait for health checks to pass
- Grant Grafana read access to PostgreSQL

### Step 4 — Open the app

| Service | URL |
|---|---|
| Web App | `http://localhost/` |
| Grafana | `http://localhost/grafana/` |

Grafana login: `admin` / *(your `GF_ADMIN_PASSWORD`)*

### Step 5 — Upload your first CSV

1. Export CSV from MyFitnessPal (see [Section 5](#5-exporting-data-from-myfitnesspal))
2. Navigate to `http://localhost/`
3. Drag and drop the CSV onto the upload zone (or click to browse)
4. Watch the import stats update

---

## 4. Configuration Reference

All configuration lives in `.env`. The values are injected into containers at runtime via `docker-compose.yml`.

### Database

| Variable | Default | Description |
|---|---|---|
| `POSTGRES_DB` | `mfp_data` | PostgreSQL database name |
| `POSTGRES_USER` | `mfp_user` | PostgreSQL username |
| `POSTGRES_PASSWORD` | *(required)* | PostgreSQL password — change before deploy |

### Grafana

| Variable | Default | Description |
|---|---|---|
| `GF_ADMIN_USER` | `admin` | Grafana admin username |
| `GF_ADMIN_PASSWORD` | *(required)* | Grafana admin password — change before deploy |
| `GRAFANA_URL` | `http://localhost/grafana` | Full URL where Grafana is accessed |

### App Settings (in `appsettings.json`)

| Key | Default | Description |
|---|---|---|
| `UploadSettings:MaxFileSizeMb` | `10` | Max CSV upload size in MB |
| `UploadSettings:AllowedExtensions` | `[".csv"]` | File extension whitelist |
| `UploadSettings:UploadPath` | `/app/uploads` | Container path for saved files |

To override app settings without rebuilding, set environment variables in `docker-compose.yml`:

```yaml
environment:
  UploadSettings__MaxFileSizeMb: "25"
```

---

## 5. Exporting Data from MyFitnessPal

MyFitnessPal provides CSV export from its website (not the mobile app).

### Steps

1. Log in at [myfitnesspal.com](https://www.myfitnesspal.com)
2. Go to **Reports** → top-right menu → **Export Data** (or navigate to `/account/data_download`)
3. Select the date range
4. Choose export type and click **Export**
5. You will receive a download or email with a `.csv` file

### Supported CSV Types (auto-detected)

The app automatically detects which type of CSV you're uploading based on the header row:

| MFP Export Type | Detection Heuristic | Tables Populated |
|---|---|---|
| **Food Log** | Header contains `Food Name` and `Meal` | `food_entries`, `daily_summaries` |
| **Exercise Log** | Header contains `Exercise Name` or `Calories Burned` | `exercise_entries`, `daily_summaries` |
| **Measurements** | Header contains `Weight` (without exercise columns) | `weight_entries` |
| **Nutrition Summary** | Header contains `Calorie Goal` | `daily_summaries` |

### Date Formats Supported

The parser handles all common MFP date formats automatically:

```
2024-01-15      (ISO 8601)
01/15/2024      (US format)
15/01/2024      (EU format)
1/15/2024       (short US)
2024/01/15      (slash-separated ISO)
```

### Tips

- Upload **food log first**, then exercise, then measurements — this ensures `daily_summaries` are computed correctly
- Duplicate rows are safely ignored (food entries use `ON CONFLICT DO NOTHING`, weight entries upsert by date)
- You can upload the same file multiple times without duplicating data

---

## 6. Database Schema

All tables use `DATE` (not timestamp) as the primary time dimension, which works natively with Grafana's `$__timeFilter` macro when cast to `::timestamp`.

### `food_entries`

| Column | Type | Description |
|---|---|---|
| `id` | bigserial PK | Auto-increment |
| `date` | date | Log date |
| `meal_name` | varchar(100) | Breakfast / Lunch / Dinner / Snacks |
| `food_name` | varchar(500) | Food item name |
| `calories` | numeric(10,2) | kcal |
| `carbohydrates` | numeric(10,2) | grams |
| `fat` | numeric(10,2) | grams |
| `protein` | numeric(10,2) | grams |
| `cholesterol` | numeric(10,2) | mg |
| `sodium` | numeric(10,2) | mg |
| `sugar` | numeric(10,2) | grams |
| `fiber` | numeric(10,2) | grams |
| `imported_at` | timestamptz | Import timestamp |

### `exercise_entries`

| Column | Type | Description |
|---|---|---|
| `id` | bigserial PK | |
| `date` | date | Log date |
| `exercise_name` | varchar(500) | Activity name |
| `minutes` | integer | Duration |
| `calories_burned` | numeric(10,2) | kcal burned |
| `sets` | varchar(50) | Optional (strength training) |
| `reps` | varchar(50) | Optional |
| `weight` | varchar(50) | Optional (weight/distance) |
| `imported_at` | timestamptz | |

### `weight_entries`

| Column | Type | Description |
|---|---|---|
| `id` | bigserial PK | |
| `date` | date | Measurement date (unique) |
| `weight` | numeric(8,2) | Weight value |
| `unit` | varchar(10) | `lbs` or `kg` |
| `imported_at` | timestamptz | |

### `daily_summaries`

Aggregated per-day view, recalculated on each import. Used by all Grafana dashboards.

| Column | Type | Description |
|---|---|---|
| `date` | date (unique) | |
| `calories_consumed` | numeric | Total food calories |
| `calories_burned` | numeric | Total exercise calories |
| `calories_goal` | numeric | From MFP goal (if available) |
| `total_carbs` | numeric | |
| `total_fat` | numeric | |
| `total_protein` | numeric | |
| `weight` | numeric | From weight_entries if available |

### `import_log`

Audit trail of every file uploaded, including warnings and row counts.

---

## 7. Grafana Dashboards

Four dashboards are provisioned automatically at startup from the JSON files in `grafana/dashboards/`.

### Overview (`mfp-overview`)

- Stat panels: Avg daily calories, Days tracked, Avg net calories, Total food entries
- Time series: Daily calories consumed vs goal line
- Bar chart: Net calorie balance (consumed − burned) per day

### Macronutrients (`mfp-macros`)

- Stat panels: Avg protein/carbs/fat per day, protein % of calories
- Time series: All three macros over time
- Donut chart: Average macro split (calories from each macro)
- Table: Top 25 foods by total calories logged

### Exercise (`mfp-exercise`)

- Stat panels: Avg kcal burned, workout sessions, total minutes, total kcal burned
- Time series: Calories burned over time
- Bar chart: Exercise minutes per day
- Table: Top 10 exercises by total calories burned

### Weight Trend (`mfp-weight`)

- Stat panels: Current weight, total change, lowest/highest
- Time series: Raw weight + 7-day moving average (using SQL window function)
- Table: Last 30 entries with per-day change
- Correlation chart: Weight vs net calories on dual axis

### Adding Custom Dashboards

1. Create a new dashboard in Grafana UI
2. Export as JSON (Dashboard settings → JSON Model)
3. Save to `grafana/dashboards/your-dashboard.json`
4. Grafana auto-reloads within 30 seconds (or restart the container)

### Useful SQL Patterns for Custom Panels

```sql
-- Grafana time range filter on a DATE column
WHERE $__timeFilter(date::timestamp)

-- 7-day rolling average
AVG(value) OVER (ORDER BY date ROWS BETWEEN 6 PRECEDING AND CURRENT ROW)

-- Calories from macros (4/4/9 rule)
(protein * 4) + (carbohydrates * 4) + (fat * 9)

-- Per-meal breakdown
SELECT meal_name, SUM(calories) FROM food_entries
WHERE $__timeFilter(date::timestamp) GROUP BY meal_name
```

---

## 8. HTTPS / SSL Setup

For a production server with a domain name, use Certbot (Let's Encrypt):

### Step 1 — Point your domain to the server

Create an A record: `dashboard.yourdomain.com → <server IP>`

### Step 2 — Stop Nginx temporarily

```bash
docker compose stop nginx
```

### Step 3 — Get the certificate

```bash
sudo apt-get install -y certbot
sudo certbot certonly --standalone -d dashboard.yourdomain.com
```

### Step 4 — Copy certs to the project

```bash
sudo cp /etc/letsencrypt/live/dashboard.yourdomain.com/fullchain.pem nginx/ssl/
sudo cp /etc/letsencrypt/live/dashboard.yourdomain.com/privkey.pem nginx/ssl/
sudo chown $USER:$USER nginx/ssl/*.pem
```

### Step 5 — Update Nginx config

Edit `nginx/conf.d/default.conf`:
- In the HTTP server block: uncomment `return 301 https://...` and remove other `location` blocks
- Uncomment the entire HTTPS server block
- Replace `yourdomain.com` with your actual domain

### Step 6 — Update `.env`

```env
SERVER_NAME=dashboard.yourdomain.com
GRAFANA_URL=https://dashboard.yourdomain.com/grafana
```

### Step 7 — Restart

```bash
docker compose up -d nginx
```

### Step 8 — Auto-renewal

```bash
# Add to crontab (crontab -e)
0 3 * * * certbot renew --quiet && \
  cp /etc/letsencrypt/live/dashboard.yourdomain.com/fullchain.pem /path/to/project/nginx/ssl/ && \
  cp /etc/letsencrypt/live/dashboard.yourdomain.com/privkey.pem /path/to/project/nginx/ssl/ && \
  docker compose -f /path/to/project/docker-compose.yml restart nginx
```

---

## 9. Useful Commands

### Manage containers

```bash
# Start all services
docker compose up -d

# Stop all services
docker compose down

# Restart a single service
docker compose restart webapp

# View live logs (all services)
docker compose logs -f

# View logs for a specific service
docker compose logs -f webapp
docker compose logs -f grafana
docker compose logs -f postgres
```

### Database operations

```bash
# Open a psql shell
docker exec -it mfp-postgres psql -U mfp_user -d mfp_data

# Quick row counts
docker exec mfp-postgres psql -U mfp_user -d mfp_data -c "
  SELECT
    (SELECT COUNT(*) FROM food_entries)    AS food_entries,
    (SELECT COUNT(*) FROM exercise_entries) AS exercise_entries,
    (SELECT COUNT(*) FROM weight_entries)   AS weight_entries,
    (SELECT COUNT(*) FROM daily_summaries)  AS daily_summaries;
"

# Backup the database
./scripts/backup.sh

# Restore from a backup
gunzip -c backups/mfp_data_20240115_120000.sql.gz | \
  docker exec -i mfp-postgres psql -U mfp_user -d mfp_data

# Clear all data (reset)
docker exec mfp-postgres psql -U mfp_user -d mfp_data -c "
  TRUNCATE food_entries, exercise_entries, weight_entries,
           daily_summaries, import_log RESTART IDENTITY;
"
```

### Build & update

```bash
# Rebuild the web app after code changes
docker compose build webapp
docker compose up -d webapp

# Pull latest base images and rebuild everything
docker compose pull
docker compose build --no-cache
docker compose up -d
```

---

## 10. Troubleshooting

### "Connection refused" or app not loading

```bash
# Check all containers are running
docker compose ps

# Check nginx logs
docker compose logs nginx

# Verify webapp is healthy
docker compose exec webapp curl -s http://localhost:8080/api/upload/status
```

### "Upload failed" or CSV not parsing

- Ensure the file extension is `.csv`
- Open the CSV in a text editor and verify it has a proper header row
- Check the app logs: `docker compose logs -f webapp`
- The import log table records all attempts — check it:

```bash
docker exec mfp-postgres psql -U mfp_user -d mfp_data \
  -c "SELECT file_name, file_type, rows_imported, success, warnings FROM import_log ORDER BY imported_at DESC LIMIT 5;"
```

### Grafana shows "No data"

1. Confirm data exists in the database (see row count command above)
2. Check the Grafana time range — set it to cover your data dates
3. Verify the datasource is connected: Grafana → Connections → Data Sources → MFP_PostgreSQL → Test
4. Check datasource provisioning used the correct password from `.env`:

```bash
docker compose exec grafana env | grep POSTGRES
```

### PostgreSQL won't start

```bash
# Check for port conflicts
sudo ss -tlnp | grep 5432

# View postgres logs
docker compose logs postgres

# If data is corrupted, remove the volume and restart (ALL DATA LOST)
docker compose down -v
docker compose up -d
```

### Grafana dashboards not appearing

Dashboards are provisioned from files. Check the provisioning logs:

```bash
docker compose logs grafana | grep -i "provision\|error\|dashboard"
```

Ensure the JSON files in `grafana/dashboards/` are valid JSON:

```bash
cat grafana/dashboards/mfp-overview.json | python3 -m json.tool > /dev/null && echo "Valid"
```

---

## 11. Development Setup

To run the ASP.NET Core app locally (without Docker) for development:

### Prerequisites

- .NET 8 SDK: [dotnet.microsoft.com/download](https://dotnet.microsoft.com/download)
- PostgreSQL running locally on port 5432

### Setup

```bash
# Create the local database
psql -U postgres -c "CREATE DATABASE mfp_data;"
psql -U postgres -c "CREATE USER mfp_user WITH PASSWORD 'mfp_password';"
psql -U postgres -c "GRANT ALL PRIVILEGES ON DATABASE mfp_data TO mfp_user;"

# Run the app
cd src/MfpDashboard
dotnet run
```

The app auto-creates all tables on first run. The development connection string is in `appsettings.Development.json`.

### Running with Docker (individual services)

Start just PostgreSQL for local development:

```bash
docker compose up -d postgres
```

Then run the webapp locally against the containerized DB.

---

## 12. Security Checklist

Before exposing this to the internet, ensure:

- [ ] `POSTGRES_PASSWORD` changed from default in `.env`
- [ ] `GF_ADMIN_PASSWORD` changed from default in `.env`
- [ ] `.env` is in `.gitignore` (it is by default — verify before committing)
- [ ] HTTPS is configured (Section 8)
- [ ] Grafana anonymous access is disabled (`GF_AUTH_ANONYMOUS_ENABLED=false` — already set)
- [ ] PostgreSQL port (5432) bound to `127.0.0.1` only (done in `docker-compose.yml`)
- [ ] Nginx `server_tokens off` is set (already in `nginx.conf`)
- [ ] Firewall allows only ports 80/443: `sudo ufw allow 80 && sudo ufw allow 443`
- [ ] Grafana `grafana_reader` password changed from `grafana_readonly_CHANGE_ME` in `scripts/postgres-init.sql`
- [ ] Regular backups scheduled (`scripts/backup.sh` via cron)
- [ ] Log rotation configured (`/etc/logrotate.d/` for Docker logs)

---

## License

MIT — use freely, modify as needed.
