# 0001 – Umbraco-only-Starterkit auf Basis der dus-umbraco-Learnings

- **Status:** Akzeptiert
- **Datum:** 2026-11-24

## Kontext

Das Projekt dus-umbraco (Umbraco 18 + Next.js + Medusa, Docker/AKS) hat viele wiederverwendbare Bausteine erprobt:
Code-first-Seeding, SEO/GEO, hybride Suche, Umbraco.AI-Setup, MCP. Für Standard-Kundenprojekte sind Headless-Frontend,
Commerce und Kubernetes jedoch Overhead.

## Entscheidung

Wir pflegen ein **schlankes Starterkit nur mit Umbraco** (Razor/MVC). Next.js, Medusa, Azure und AKS sind nicht
enthalten. Docker dient als lokales Dev- und Test-Setup, auch mit SQL Server ([0003](0003-lokale-entwicklung-sqlite-oder-docker-sql-server.md)), und ist für den Betrieb nicht vorgeschrieben.

## Konsequenzen

- Positiv: ein Deployable, kurze Einarbeitung, `dotnet run` genügt lokal.
- Negativ: Headless-Kanäle (Apps, SPA) brauchen später zusätzliche Arbeit, siehe [0002](0002-razor-ohne-delivery-api.md).
