# AGENTS.md – Opten Umbraco Starter

Gilt für alle KI-Coding-Agenten (Claude Code, Codex, Copilot, Cursor …). Claude-spezifische Ergänzungen stehen in
[CLAUDE.md](CLAUDE.md). Menschen lesen zuerst die [README.md](README.md).

## Projekt in einem Satz

Umbraco 18 (.NET 10) mit Razor/MVC-Rendering, kein Headless, keine Content Delivery API. Inhaltsmodell, Starter-Inhalte
und Umbraco.AI werden per Code geseedet. Seiten sind SEO- und GEO-optimiert (llms.txt, Markdown-Zwillinge, schema.org),
die Suche ist hybrid (Vektoren + Volltext). Frontend: Tailwind CSS v4.

## Befehle

| Zweck | Befehl |
| --- | --- |
| Abhängigkeiten | `npm install` (setzt auch die Git-Hooks) · `dotnet restore` |
| Starten (lokal, SQLite) | `dotnet run --project src/Opten.Web` → https://localhost:44380 |
| CSS live bauen | `npm run watch:css` |
| Build | `dotnet build Opten.Umbraco.slnx` |
| Tests | `dotnet test Opten.Umbraco.slnx` |
| Format prüfen/fixen | `dotnet format Opten.Umbraco.slnx --verify-no-changes` / ohne Flag |
| Docker | `docker compose --env-file .env -f docker/docker-compose.yml up --build` → http://localhost:8080 |
| Datenbank zurücksetzen (lokal) | App stoppen, `rm -rf src/Opten.Web/umbraco/Data` |

## Struktur

```
src/Opten.Web/
  Program.cs            Startup (.env laden, Umbraco, Endpunkte)
  Configuration/        .env-Loader, lokale HTTP-Ausnahme
  Site/                 Kulturen (de-DE, en-US), UI-Texte (Dictionary), View-Helfer, SiteOptions
  Seeding/              Seeder-Template: ISeedStep + SeedRunner
    Schema/             Deklaratives Inhaltsmodell (Aliases, Element-/Seitentypen, SEO/GEO-Tabs)
    Content/            Starter-Inhalte (StarterContent.cs), Block-Serialisierung
    Steps/              Versionierte Seed-Steps (Languages, Schema, Dictionary, StarterContent)
    Ai/                 Umbraco.AI: Connection, Brand Voice, Prompts, Copilot, Embeddings
    Mcp/                API-User für den Umbraco Developer MCP
  Seo/                  Meta-Tags, hreflang, JSON-LD (schema.org), sitemap.xml, robots.txt
  Geo/                  llms.txt, llms-full.txt, Markdown-Zwilling jeder Seite (/seite.md)
  Search/               Hybride semantische Suche, /api/search, Suchseite
  Views/                Razor-Templates, Partials, Block-Grid-Komponenten
  Styles/site.css       Tailwind-Quelle → wwwroot/css/site.css (generiert, nicht committen)
tests/Opten.Web.Tests/  xUnit-Tests (reine Logik ohne Umbraco-Laufzeit)
docs/                   Dokumentation + ADRs (docs/adr)
docker/                 Dockerfile + Compose (SQLite, optional SQL Server)
.githooks/              pre-commit / commit-msg / post-commit
.github/workflows/      CI (Build, Tests, Format, CSS), AI Security Review, Release-Artefakt
```

## Harte Regeln

1. **Keine Secrets im Repo.** Konfiguration über `.env` (Vorlage: `.env.example`). API-Keys gehören nur unter
   `Umbraco:AI:Secrets:*`. Der Pre-Commit-Hook blockt `.env` und offensichtliche Keys.
2. **Inhaltsmodell nur per Code ändern.** Neue Properties, Elemente und Seitentypen in `Seeding/Schema/*Definitions.cs`.
   Bestehende Installationen bekommen Änderungen über einen **neuen oder hochversionierten Seed-Step**
   (`ISeedStep.Version` erhöhen), nicht über Klicks im Backoffice.
3. **Aliase nie als String-Literal.** Immer `DocTypes`, `Elements`, `Props` aus `Seeding/Schema/Aliases.cs`.
4. **Kein Headless.** Content Delivery API bleibt aus (`DeliveryApi:Enabled=false`), Rendering über Razor.
   Begründung: [ADR 0002](docs/adr/0002-razor-ohne-delivery-api.md).
5. **SEO/GEO gilt für jeden Seitentyp.** Neue Seitentypen erben automatisch `seoComposition` und `geoComposition`
   (siehe `SchemaSeeder`). Einen schema.org-Standardtyp in `StructuredDataBuilder.DefaultPageTypes` ergänzen.
6. **Tailwind-Klassen nur in `Views/` oder `Site/`.** Andere Ordner werden nicht gescannt.
7. **Architekturentscheidungen als ADR** (Skill `/adr`, Vorlage `docs/adr/0000-template.md`).
8. **Datenbank-agnostisch bleiben.** Lokal laufen SQLite (`dotnet run`) und SQL Server (Docker) parallel
   ([ADR 0003](docs/adr/0003-lokale-entwicklung-sqlite-oder-docker-sql-server.md), offen). Kein rohes SQL, Änderungen auf beiden Wegen testen.
9. **Security-Findings nicht umgehen.** HIGH-Findings des Pre-Commit-Checks beheben, nicht mit `--no-verify` oder
   `AI_SECURITY_ALLOW_HIGH=1` überspringen, außer der Mensch entscheidet es ausdrücklich ([ADR 0008](docs/adr/0008-ai-security-review-in-ci.md)).
10. **Conventional Commits**: `feat|fix|refactor|docs|test|chore|perf|ci|build|style|revert(scope)?: beschreibung`.
   Der `commit-msg`-Hook prüft das Format.

## Neues Block-Grid-Modul

1. `ElementDefinitions.Modules` erweitern (Alias in `Elements`, Properties mit `Props`).
2. Gruppe in `BlockGridConfigFactory.GroupByModule` zuordnen.
3. View `Views/Partials/blockgrid/Components/{alias}.cshtml` anlegen.
4. Bestehende Installationen: neuen Seed-Step schreiben, der Element + Block-Grid-Konfiguration ergänzt.
5. GEO: Der Markdown-Export (`BlockMarkdownWriter`) arbeitet generisch, eigener Code ist meist nicht nötig.

## Verifikation vor „fertig“

- `dotnet build` ohne Warnungen, `dotnet test` grün, `dotnet format --verify-no-changes` sauber.
- Seite tatsächlich im Browser öffnen (Startseite, geänderte Seite, `/llms.txt`, `/<seite>.md`).
- Bei Schema-Änderungen: einmal mit leerer Datenbank starten (`rm -rf src/Opten.Web/umbraco/Data`).
