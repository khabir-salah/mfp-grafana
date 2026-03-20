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

1. [Project Structure](#1-project-structure)
2. [Quick Start](#2-quick-start)
3. [Configuration Reference](#3-configuration-reference)
4. [Exporting Data from MyFitnessPal](#4-exporting-data-from-myfitnesspal)
5. [Database Schema](#5-database-schema)


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

