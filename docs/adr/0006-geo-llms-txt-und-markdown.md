# 0006 – GEO über llms.txt, Markdown-Zwillinge und schema.org pro Seitentyp

- **Status:** Akzeptiert
- **Datum:** 2026-11-24

## Entscheidung

Jede Seite bekommt über `geoComposition` KI-Zusammenfassung, Kernfakten und einen schema.org-Typ. Die Website
liefert `/llms.txt`, `/llms-full.txt` und `/{seite}.md`. KI-Suchagenten sind in robots.txt immer erlaubt,
Trainings-Crawler sind optional sperrbar.

## Konsequenzen

- Positiv: Inhalte sind für KI-Assistenten zitierfähig, ohne dass eine öffentliche Content-API nötig ist.
- Negativ: Die Redaktion pflegt zusätzliche Felder. Die Fallbacks auf die Meta-Beschreibung reduzieren den Aufwand.
