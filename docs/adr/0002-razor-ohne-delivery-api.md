# 0002 – Razor-Rendering ohne Content Delivery API

- **Status:** Akzeptiert
- **Datum:** 2026-11-24

## Kontext

Das frühere Headless-Projekt ([0001](0001-umbraco-only-starterkit.md)) lieferte Inhalte über die Content Delivery API an Next.js. Das brachte Preview-Signierung, Revalidierung,
API-Keys und doppelte Darstellungslogik mit sich.

## Entscheidung

Seiten werden **serverseitig mit Razor** gerendert. `Umbraco:CMS:DeliveryApi:Enabled` ist `false`, und
`AddDeliveryApi()` wird nicht aufgerufen.

## Betrachtete Optionen

- Headless (Delivery API + JS-Frontend): flexibel, aber zweite Codebasis und zusätzlicher Betrieb.
- **Razor:** eine Codebasis, Backoffice-Vorschau ohne Zusatzaufwand, SEO/GEO serverseitig.

## Konsequenzen

- Positiv: weniger Angriffsfläche (keine öffentliche Content-API), einfacheres Caching.
- Negativ: Wer einen weiteren Kanal braucht, muss die Delivery API gezielt aktivieren (neue ADR).
- Agenten bekommen maschinenlesbare Inhalte stattdessen über llms.txt und Markdown-Zwillinge ([0006](0006-geo-llms-txt-und-markdown.md)).
