# 0003 – Lokale Entwicklung wahlweise mit dotnet run + SQLite oder Docker + SQL Server

- **Status:** Vorgeschlagen (offen) – beide Wege werden bis zur Entscheidung gleichwertig unterstützt
- **Datum:** 2026-11-24
- **Entscheider:** offen

## Kontext

Die Website muss lokal auf zwei Arten laufen können:

- **nativ** mit `dotnet run` ohne weitere Infrastruktur, damit an Views, CSS und Seedern schnell iteriert werden kann
- **produktionsnah** gegen dieselbe Datenbank-Engine wie Staging und Produktion (SQL Server), etwa für Tests mit
  Datenbank-Dumps

Das Starterkit bringt dafür ein Docker-Dev-Setup mit (`docker/docker-compose.yml` +
`docker/docker-compose.sqlserver.yml`, SQL Server 2022). Welcher der beiden Wege Projektstandard wird, ist offen.
Dafür fehlen Erfahrungswerte aus echten Kundenprojekten: Teamgröße, Rechnerausstattung (Apple Silicon emuliert
SQL Server nur), Bedarf an produktionsnahen Tests und Umgang mit Datenbank-Dumps.

Rahmenbedingungen: Umbraco 18.3, .NET 10, Umbraco.AI.Search 18.0.7 (unterstützt SQLite und SQL Server),
SQL Server 2022 Developer Edition im Container (`linux/amd64`).

## Entscheidung (vorläufig)

Bis zur endgültigen Entscheidung werden **beide Wege gleichwertig unterstützt**. CI baut bisher nur das Image,
ein Lauf gegen SQL Server steht noch aus (siehe Folgearbeiten).

| Weg | Start | Datenbank | Gedacht für |
| --- | --- | --- | --- |
| **A – nativ** | `dotnet run --project src/Opten.Web` | SQLite (`umbraco/Data/Umbraco.sqlite.db`) | schnelle Iteration an Views, CSS und Seedern |
| **B – Docker** | `docker compose --env-file .env -f docker/docker-compose.yml -f docker/docker-compose.sqlserver.yml up --build` | SQL Server 2022 (Container) | produktionsnahe Tests, Datenbank-Dumps, Teams ohne lokales .NET SDK |

Zusätzlich gibt es Docker mit SQLite (nur `docker/docker-compose.yml`) als schnellen Smoke-Test des Container-Images.
**Staging und Produktion laufen unabhängig davon immer auf SQL Server.**

Regeln, solange die ADR offen ist:

- Code darf nicht datenbankspezifisch sein. Kein rohes SQL; Seeder und Suche nutzen nur Umbraco- und
  Umbraco.AI-Services.
- Neue Features werden auf beiden Wegen getestet, mindestens einmal mit leerer Datenbank.
- Neue Konfiguration wird über `.env` gesteuert, damit sie in beiden Wegen gleich funktioniert.

## Betrachtete Optionen

### Option A – `dotnet run` + SQLite als Standard

- ✅ Kein Docker nötig, Start in Sekunden, Debugging direkt in Rider oder VS Code.
- ✅ Datenbank ist eine Datei: Zurücksetzen mit `rm -rf src/Opten.Web/umbraco/Data`.
- ❌ Abweichung zur Produktion (SQL Server): Sperrverhalten, Collation, Performance und Vektorsuche verhalten sich
  anders, Fehler können erst auf Staging auffallen.
- ❌ Datenbank-Dumps aus Staging oder Produktion lassen sich lokal nicht direkt einspielen.

### Option B – Docker + SQL Server als Standard

- ✅ Gleiche Datenbank-Engine wie Produktion, Dumps (`.bak`) direkt nutzbar.
- ✅ Einheitliche Umgebung für alle im Team, kein lokales .NET SDK nötig.
- ❌ Auf Apple Silicon wird SQL Server emuliert (`linux/amd64`): langsamerer Start, mehr RAM.
- ❌ Langsamere Iteration (Image-Build) und umständlicheres Debugging.

### Option C – beide Wege dauerhaft parallel

- ✅ Jede Entwicklerin und jeder Entwickler wählt passend zur Aufgabe.
- ❌ Doppelter Pflege- und Testaufwand, und die Doku muss beide Wege aktuell halten.

## Konsequenzen

- Positiv: Niemand ist blockiert. Schnelle Arbeit läuft nativ, produktionsnahe Tests laufen in Docker mit SQL Server.
- Negativ / Risiken:
  - Fehler, die nur auf einer Datenbank auftreten, fallen auf, sobald beide Wege genutzt werden. Dafür kostet es
    zusätzliche Testzeit.
  - Zwei Wege bedeuten mehr Pflege bei Doku (README, `docs/guides`), `.env.example` und Compose-Dateien.
  - SQLite ist nicht für Load-Balancing oder parallele Schreiblast gedacht. Für Staging und Produktion ist es keine
    Option.
- Folgearbeiten bis zur Entscheidung:
  - [ ] CI um einen Job erweitern, der das Image gegen SQL Server startet und einen Smoke-Test (`/de/`, `/llms.txt`)
        ausführt.
  - [ ] Erfahrungen aus den ersten zwei Projekten sammeln: Startzeiten, Probleme und Präferenzen im Team.
  - [ ] Danach entscheiden: Weil diese ADR dann bereits gilt, wird sie nicht umgeschrieben, sondern durch eine neue
        ADR abgelöst, die einen Weg als Standard festlegt.

## Links

- Code: `src/Opten.Web/appsettings.json` (SQLite-Default), `docker/docker-compose.yml`, `docker/docker-compose.sqlserver.yml`
- Konfiguration: `.env.example` (`ConnectionStrings__umbracoDbDSN`, `DB_SA_PASSWORD`, `WEB_PORT`)
- Doku: [README, Abschnitte 2 und 3](../../README.md#2-schnellstart-lokal-ohne-docker), [Architektur](../architecture/overview.md)
