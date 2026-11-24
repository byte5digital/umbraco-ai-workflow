# Dokumentation

Projektdokumentation des Umbraco AI Starterkit. Jede Datei behandelt **ein** Thema und ist in Markdown geschrieben.
Vorlagen für neue Seiten liegen in [`_templates/`](_templates/).

| Bereich | Inhalt |
| --- | --- |
| [Architektur](architecture/overview.md) | Bausteine, Request-Fluss, Verzeichnisstruktur |
| [Seeding](guides/seeding.md) | Inhaltsmodell per Code, Seed-Steps, Upgrades bestehender Installationen |
| [SEO & GEO](guides/seo-geo.md) | Meta-Tags, schema.org pro Seitentyp, llms.txt, Markdown-Zwillinge, robots.txt |
| [Semantische Suche](guides/search.md) | Hybride Suche, API, Index-Verhalten |
| [Umbraco AI](guides/umbraco-ai.md) | Connection, Brand Voice, Prompts, Copilot, MCP |
| [Frontend & Tailwind](guides/frontend.md) | Views, Block-Grid-Komponenten, Tailwind-Build |
| [CI/CD & Hooks](guides/ci-cd.md) | GitHub Actions, AI Security Review, Git-Hooks |
| [Architekturentscheidungen](adr/README.md) | ADRs (MADR) |

## Konventionen

- Sprache: Deutsch. Code, Aliase und Commit-Messages: Englisch.
- Neue Feature-Doku mit [`_templates/feature.md`](_templates/feature.md), Betriebsanleitungen mit
  [`_templates/runbook.md`](_templates/runbook.md), Entscheidungen als ADR (Skill `/adr`).
- Doku wird im selben PR wie der Code geändert. Veraltete Doku ist ein Bug.
