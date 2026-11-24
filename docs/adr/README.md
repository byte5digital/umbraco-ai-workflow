# Architecture Decision Records

Format: [MADR](https://adr.github.io/madr/), Vorlage: [0000-template.md](0000-template.md).
Neue ADR: Skill `/adr` in Claude Code oder `bash .claude/skills/adr/new-adr.sh "Titel"`.

| Nr. | Entscheidung | Status |
| --- | --- | --- |
| [0001](0001-umbraco-only-starterkit.md) | Umbraco AI Starterkit nur mit Umbraco, ohne Headless-Frontend und Commerce | Akzeptiert |
| [0002](0002-razor-ohne-delivery-api.md) | Razor-Rendering ohne Content Delivery API | Akzeptiert |
| [0003](0003-lokale-entwicklung-sqlite-oder-docker-sql-server.md) | Lokale Entwicklung wahlweise mit dotnet run + SQLite oder Docker + SQL Server | **Offen** (beide Wege unterstützt) |
| [0004](0004-code-first-seeding.md) | Code-first-Seeding mit versionierten Seed-Steps | Akzeptiert |
| [0005](0005-umbraco-ai-mit-secret-referenz.md) | Umbraco.AI mit OpenAI, API-Key nur als Konfigurationsreferenz | Akzeptiert |
| [0006](0006-geo-llms-txt-und-markdown.md) | GEO über llms.txt, Markdown-Zwillinge und schema.org pro Seitentyp | Akzeptiert |
| [0007](0007-hybride-semantische-suche.md) | Hybride Suche (Vektor + Volltext, RRF) | Akzeptiert |
| [0008](0008-ai-security-review-in-ci.md) | Zweistufiger AI Security Review: lokal im Pre-Commit-Hook und in CI mit Issues | Akzeptiert |
