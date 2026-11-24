# 0001 – Umbraco AI Starterkit nur mit Umbraco, ohne Headless-Frontend und Commerce

- **Status:** Akzeptiert
- **Datum:** 2026-11-24

## Kontext

Ein früheres Headless-Projekt (Umbraco 18 + Next.js + Medusa, Docker/Kubernetes) hat viele wiederverwendbare Bausteine
erprobt: Code-first-Seeding, SEO/GEO, hybride Suche, Umbraco.AI-Setup, MCP. Für Standardprojekte sind Headless-Frontend,
Commerce und Kubernetes jedoch Overhead. Der Mehrwert liegt in den KI-Bausteinen, nicht in der Infrastruktur.

## Entscheidung

Wir pflegen das **Umbraco AI Starterkit** als schlankes Starterkit nur mit Umbraco (Razor/MVC). Next.js, Medusa und
Cloud-Vorgaben (Azure, Kubernetes) sind nicht enthalten. Docker dient als lokales Dev- und Test-Setup, auch mit SQL Server ([0003](0003-lokale-entwicklung-sqlite-oder-docker-sql-server.md)), und ist für den Betrieb nicht vorgeschrieben.

## Konsequenzen

- Positiv: ein Deployable, kurze Einarbeitung, `dotnet run` genügt lokal.
- Negativ: Headless-Kanäle (Apps, SPA) brauchen später zusätzliche Arbeit, siehe [0002](0002-razor-ohne-delivery-api.md).
