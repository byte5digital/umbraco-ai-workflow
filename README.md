# Opten Umbraco Starter

Starterkit für Umbraco-Projekte. Es übernimmt die Learnings aus **dus-umbraco**, nur ohne Headless-Frontend, Commerce und
Cloud-Infrastruktur.

| | |
| --- | --- |
| **CMS** | Umbraco 18.3 auf .NET 10, Razor/MVC-Rendering (keine Content Delivery API) |
| **Datenbank** | SQLite lokal (null Setup), SQL Server per Connection-String |
| **Frontend** | Tailwind CSS v4 (+ Typography), barrierearme Razor-Views, Block Grid |
| **Inhaltsmodell** | Code-first: Seeder-Template mit versionierten Seed-Steps, zweisprachige Starter-Inhalte (DE/EN) |
| **SEO** | Meta, Canonical, hreflang, Open Graph, `sitemap.xml`, `robots.txt` |
| **GEO** | KI-Zusammenfassung, Kernfakten und schema.org-Typ **pro Seitentyp**, `llms.txt`, `llms-full.txt`, Markdown-Zwilling jeder Seite |
| **KI** | Umbraco.AI (OpenAI): Brand Voice, Prompt-Aktionen, Copilot, **semantische Suche** (hybrid), Umbraco MCP für Claude Code |
| **Qualität** | `AGENTS.md`/`CLAUDE.md`, ADR-Skill, Doku-Templates, Pre-/Post-Commit-Hooks, GitHub Actions inkl. **AI Security Review** |
| **Docker** | optional zum Testen (SQLite oder SQL Server) |

---

## Inhalt

1. [Voraussetzungen](#1-voraussetzungen)
2. [Schnellstart lokal (ohne Docker)](#2-schnellstart-lokal-ohne-docker)
3. [Schnellstart mit Docker](#3-schnellstart-mit-docker)
4. [Konfiguration (.env)](#4-konfiguration-env)
5. [Umbraco AI & semantische Suche aktivieren](#5-umbraco-ai--semantische-suche-aktivieren)
6. [Was nach dem ersten Start da ist](#6-was-nach-dem-ersten-start-da-ist)
7. [Täglicher Workflow](#7-täglicher-workflow)
8. [Neues Kundenprojekt aus dem Starter](#8-neues-kundenprojekt-aus-dem-starter)
9. [Git-Hooks & CI/CD](#9-git-hooks--cicd)
10. [Arbeiten mit KI-Agenten](#10-arbeiten-mit-ki-agenten)
11. [Projektstruktur](#11-projektstruktur)
12. [Troubleshooting](#12-troubleshooting)

---

## 1. Voraussetzungen

| Tool | Version | Prüfen |
| --- | --- | --- |
| .NET SDK | **10.0** (siehe `global.json`) | `dotnet --list-sdks` |
| Node.js | **22+** (Tailwind, Git-Hooks, Umbraco MCP) | `node -v` |
| Git | aktuell | `git --version` |
| Docker Desktop | optional | `docker compose version` |

.NET 10 installieren: <https://dotnet.microsoft.com/download/dotnet/10.0> oder per Skript ohne Admin-Rechte:

```bash
curl -fsSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh
bash /tmp/dotnet-install.sh --channel 10.0 --install-dir "$HOME/.dotnet"
export PATH="$HOME/.dotnet:$PATH"   # in ~/.zshrc übernehmen
```

## 2. Schnellstart lokal (ohne Docker)

```bash
git clone <repo-url> opten-umbraco && cd opten-umbraco

# 1. Konfiguration anlegen und Admin-Zugang anpassen
cp .env.example .env

# 2. Frontend-Tooling + Git-Hooks
npm install

# 3. HTTPS-Entwicklerzertifikat (einmalig pro Rechner)
dotnet dev-certs https --trust

# 4. Starten
dotnet run --project src/Opten.Web
```

- Website: <https://localhost:44380> (leitet auf `/de/` weiter)
- Backoffice: <https://localhost:44380/umbraco>, Login mit `Umbraco__CMS__Unattended__UnattendedUserEmail` / `…Password` aus `.env`

Beim ersten Start installiert Umbraco unbeaufsichtigt die SQLite-Datenbank (`src/Opten.Web/umbraco/Data/`). Danach
legen die Seed-Steps Sprachen, Inhaltsmodell, Dictionary und Starter-Seiten an. Im Log stehen Zeilen wie
`Seed step schema v1 completed`.

CSS während der Entwicklung live bauen (zweites Terminal):

```bash
npm run watch:css
```

> `dotnet build` baut das CSS automatisch mit, sobald `node_modules` existiert.

## 3. Schnellstart mit Docker

```bash
cp .env.example .env                       # falls noch nicht geschehen
docker compose --env-file .env -f docker/docker-compose.yml up --build
```

- Website: <http://localhost:8080>, Backoffice: <http://localhost:8080/umbraco>
- Daten (SQLite, Media, Logs) liegen in Docker-Volumes und überleben Neustarts.
- Login über HTTP ist nur erlaubt, weil Compose `ASPNETCORE_ENVIRONMENT=Development` und
  `LocalDev__AllowHttpBackoffice=true` setzt. Das gilt nur für lokales Testen.

Produktionsnah mit **SQL Server 2022** statt SQLite:

```bash
docker compose --env-file .env -f docker/docker-compose.yml -f docker/docker-compose.sqlserver.yml up --build
```

Komplett zurücksetzen (Datenbank, Media und Logs löschen):

```bash
docker compose --env-file .env -f docker/docker-compose.yml down -v
```

## 4. Konfiguration (.env)

Alle Einstellungen stehen in `.env` (Vorlage mit Kommentaren: [`.env.example`](.env.example)). Die Schlüssel folgen der
ASP.NET-Konvention: `__` trennt die Abschnitte, `Seed__StarterContent` entspricht also `Seed:StarterContent` in `appsettings.json`.

| Variable | Zweck |
| --- | --- |
| `Umbraco__CMS__Unattended__*` | Erstinstallation und erster Admin |
| `ConnectionStrings__umbracoDbDSN` (+ `_ProviderName`) | SQL Server statt SQLite (optional) |
| `Site__Name` | Fallback-Website-Name (sonst: Organisation auf der Startseite) |
| `Seed__Enabled`, `Seed__StarterContent` | Seeding an/aus, Starter-Seiten an/aus |
| `Umbraco__AI__Secrets__OpenAIApiKey` | aktiviert Umbraco AI und die Vektorsuche |
| `AiSetup__ChatModel`, `AiSetup__EmbeddingModel` | Modelle der KI-Profile |
| `McpApiUser__ClientSecret`, `UMBRACO_MCP_BASE_URL` | Umbraco MCP für Claude Code |

Lokal liest die App `.env` selbst (`Configuration/DotEnvFile.cs`), Docker übergibt die Datei per `env_file`. Echte
Umgebungsvariablen (Shell, CI, Server) haben immer Vorrang. `.env.example` enthält lauffähige Werte für die lokale
Entwicklung. Leer bleiben nur LLM-API-Keys, die ausschließlich in die eigene `.env` gehören. **`.env` wird nie committet.** Der Pre-Commit-Hook
blockiert das zusätzlich.

## 5. Umbraco AI & semantische Suche aktivieren

1. OpenAI-API-Key in `.env` eintragen: `Umbraco__AI__Secrets__OpenAIApiKey=sk-…`
2. App neu starten. Die Steps `umbraco-ai` und `umbraco-ai-search` legen dann an:
   Connection (Key nur als Referenz), Brand Voice, Redaktions-Profil, Prompt-Aktionen (SEO, GEO, Text, Übersetzung),
   Copilot „Redaktionsassistent“, den KI-Button im Rich-Text-Editor, das Embedding-Profil und den gefüllten Vektorindex.
3. Prüfen: Backoffice → **AI** (Connections, Profiles, Prompts, Agents). Auf der Suchseite zeigen Treffer jetzt
   `semantic` und/oder `text`.

Ohne Key läuft alles außer den KI-Funktionen. Die Suche nutzt dann nur den Volltext. Details: [docs/guides/umbraco-ai.md](docs/guides/umbraco-ai.md).

## 6. Was nach dem ersten Start da ist

| URL | Inhalt |
| --- | --- |
| `/de/`, `/en/` | Startseite (Bühne, Teaser, News, CTA) |
| `/de/ueber-uns/`, `/de/leistungen/`, `/de/kontakt/` | Inhaltsseiten (inkl. FAQ → schema.org `FAQPage`) |
| `/de/news/` | News-Übersicht mit 2 Artikeln |
| `/de/suche/?q=team` | Suchseite (hybride Suche) |
| `/api/search?q=team&culture=de-DE` | Such-API (JSON, rate-limitiert) |
| `/sitemap.xml`, `/robots.txt` | mit hreflang bzw. KI-Crawler-Steuerung |
| `/llms.txt`, `/en/llms.txt`, `/llms-full.txt` | GEO: Index und Volltext für KI-Agenten |
| `/de/ueber-uns.md` | Markdown-Zwilling jeder Seite |

Im Backoffice hat jeder Seitentyp die Tabs **Inhalt · SEO · Social Media · GEO & KI**, die Startseite zusätzlich
**Website-Einstellungen** (Navigation, Footer, Organisation, llms.txt, KI-Crawler).

## 7. Täglicher Workflow

```bash
dotnet run --project src/Opten.Web        # Seite
npm run watch:css                          # Tailwind
dotnet build Opten.Umbraco.slnx            # Build
dotnet test Opten.Umbraco.slnx             # Tests
dotnet format Opten.Umbraco.slnx           # Formatieren
```

**Inhaltsmodell ändern:** immer im Code (`src/Opten.Web/Seeding/Schema/`), nie im Backoffice. Für eine frische
Installation genügt ein Neustart mit leerer Datenbank (`rm -rf src/Opten.Web/umbraco/Data`). Bestehende Installationen
brauchen einen neuen Seed-Step, siehe [docs/guides/seeding.md](docs/guides/seeding.md).

**Neues Block-Grid-Modul:** `ElementDefinitions.Modules` erweitern, View unter
`Views/Partials/blockgrid/Components/{alias}.cshtml` anlegen. Die Checkliste steht in [AGENTS.md](AGENTS.md#neues-block-grid-modul).

## 8. Neues Kundenprojekt aus dem Starter

1. Repo kopieren bzw. als Template verwenden, dann in `appsettings.json` eine **neue** `Umbraco:CMS:Global:Id` setzen (`uuidgen`).
2. Namen anpassen: `Site__Name`, `SiteOptions.DataTypePrefix`, Marke in `Styles/site.css` (`--color-brand-*`).
3. Sprachen: `Site/Cultures.cs` und `Site/UiTexts.cs`.
4. Inhalte: `Seeding/Content/StarterContent.cs` durch die echte Seitenstruktur ersetzen oder `Seed__StarterContent=false`.
5. KI: `Seeding/Ai/AiBrandVoice.cs`, Prompts in `AiPromptDefinitions.cs`.
6. Erste ADR zum Projektkontext schreiben (`/adr` bzw. `bash .claude/skills/adr/new-adr.sh "…"`).
7. GitHub: Secret `ANTHROPIC_API_KEY` für den AI Security Review anlegen, Branch-Protection auf `main` mit den
   Checks *CI* und *Security*.
8. Deployment in `.github/workflows/release.yml` ergänzen (bewusst ohne Cloud-Vorgabe).

## 9. Git-Hooks & CI/CD

**Git-Hooks** (`.githooks/`, aktiviert durch `npm install`, manuell mit `npm run hooks:install`):

| Hook | Wirkung |
| --- | --- |
| `pre-commit` | blockt `.env`-Dateien und Secrets, prüft das C#-Format der geänderten Dateien und die JSON-Syntax, **AI Security Check des gestagten Diffs** per Claude Code CLI (HIGH blockiert, MEDIUM/LOW warnen; über Claude committet erscheint eine TL;DR-Tabelle in der Antwort) |
| `commit-msg` | Conventional Commits (`feat(search): …`) |
| `post-commit` | Hinweise: Schema geändert → Seed-Step? Infrastruktur geändert → ADR? |

**GitHub Actions** (`.github/workflows/`):

| Workflow | Wann | Was |
| --- | --- | --- |
| `ci.yml` | Push auf `main`, PRs | Tailwind, Format-Check, Build (Warnungen = Fehler), Tests, verwundbare NuGet- und npm-Pakete, Publish-Artefakt, Docker-Build |
| `security.yml` | PRs, wöchentlich | **AI Security Review** mit Claude: Funde als PR-Kommentar **und als Issue mit Severity-Label**, gitleaks, Dependency Review |
| `release.yml` | Tag `v*` | getestetes Publish-ZIP als GitHub Release; Deploy-Schritt pro Projekt ergänzen |

Für den AI Security Review unter *Settings → Secrets and variables → Actions* das Secret `ANTHROPIC_API_KEY` anlegen.
Ohne Secret wird der Job mit einem Hinweis übersprungen. Projektspezifische Prüfhinweise stehen in
[`.github/security-review-instructions.md`](.github/security-review-instructions.md). Details: [docs/guides/ci-cd.md](docs/guides/ci-cd.md).

## 10. Arbeiten mit KI-Agenten

- **[AGENTS.md](AGENTS.md):** Regeln, Befehle und Struktur für alle Agenten. **[CLAUDE.md](CLAUDE.md)** ergänzt die
  Claude-spezifischen Punkte.
- **Umbraco MCP:** funktioniert mit den Werten aus `.env.example` direkt (nur in der Development-Umgebung). Seite starten, Claude Code im Repo
  öffnen. `.mcp.json` verbindet `@umbraco-cms/mcp-dev` automatisch (nur lokal nutzen, der API-User ist Admin).
- **Skill `/adr`:** legt die nächste ADR aus der Vorlage an und trägt sie in `docs/adr/README.md` ein.
- **Claude-Code-Hook:** formatiert jede geänderte `.cs`-Datei (`.claude/settings.json`).
- **Doku-Templates:** `docs/_templates/feature.md` und `runbook.md`.

## 11. Projektstruktur

```
├── src/Opten.Web/            Umbraco-Website (einziges Deployable)
│   ├── Configuration/        .env-Loader, lokale HTTP-Ausnahme
│   ├── Site/                 Kulturen, UI-Texte, View-Helfer
│   ├── Seeding/              Seeder-Template (Schema, Content, Steps, Ai, Mcp)
│   ├── Seo/                  Meta, JSON-LD, Sitemap, robots.txt
│   ├── Geo/                  llms.txt, Markdown-Zwillinge
│   ├── Search/               hybride semantische Suche
│   ├── Views/                Razor (Templates, Partials, Block-Grid-Module)
│   ├── Styles/site.css       Tailwind-Quelle
│   └── SeedData/images/      Bilder der Starter-Inhalte
├── tests/Opten.Web.Tests/    xUnit
├── docs/                     Guides, Architektur, ADRs, Templates
├── docker/                   Dockerfile, Compose (SQLite / SQL Server)
├── scripts/umbraco-mcp.sh    Umbraco MCP für Claude Code
├── .githooks/                pre-commit, commit-msg, post-commit
├── .github/                  Workflows, Dependabot, PR-Template, Security-Review-Hinweise
├── .claude/                  Settings, Hooks, Skill „adr“
├── AGENTS.md · CLAUDE.md     Regeln für KI-Agenten
└── .env.example              Konfigurationsvorlage
```

## 12. Troubleshooting

| Problem | Lösung |
| --- | --- |
| `A compatible .NET SDK was not found` | .NET 10 SDK installieren (siehe [1](#1-voraussetzungen)) bzw. `PATH` prüfen |
| Seite ohne Styles | `npm install && npm run build:css` |
| Backoffice-Login klappt nicht (Docker/HTTP) | `ASPNETCORE_ENVIRONMENT=Development` und `LocalDev__AllowHttpBackoffice=true` setzen (macht Compose bereits) |
| Starter-Inhalte fehlen | Werden nur bei leerem Inhaltsbaum angelegt. Lokal `rm -rf src/Opten.Web/umbraco/Data`, Docker `down -v` |
| Suche findet direkt nach dem Start nichts | Examine baut den Index nach dem Kaltstart im Hintergrund auf. Ein paar Sekunden warten |
| Nur `text`-Treffer, kein `semantic` | OpenAI-Key fehlt, oder der Vektorindex wird noch aufgebaut (Log: `AI search setup`) |
| Seed-Step schlägt fehl | Log prüfen (`Seed step … failed`), Ursache beheben, neu starten. Der Step wird wiederholt |
| Commit wird abgelehnt | Hook-Meldung lesen. Notfalls `git commit --no-verify`, CI prüft trotzdem |
