# 0007 – Hybride Suche (Vektor + Volltext, Reciprocal Rank Fusion)

- **Status:** Akzeptiert
- **Datum:** 2026-11-24

## Entscheidung

Die Website-Suche kombiniert **Umbraco.AI.Search** (Bedeutung) und **Examine** (exakte Begriffe) per RRF.

## Konsequenzen

- Positiv: findet Synonyme und Umschreibungen und trotzdem Namen, Codes und Zahlen. Ohne Key bleibt die Volltextsuche verfügbar.
- Negativ: Jede ungecachte Anfrage kostet ein Embedding, deshalb gibt es Rate Limit und Cache.
