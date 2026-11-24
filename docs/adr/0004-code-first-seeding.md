# 0004 – Code-first-Seeding mit versionierten Seed-Steps

- **Status:** Akzeptiert
- **Datum:** 2026-11-24

## Kontext

Im früheren Headless-Projekt ([0001](0001-umbraco-only-starterkit.md)) wuchs das Seeding zu vielen einzelnen Upgrade-Handlern mit eigenen Zustandsschlüsseln.

## Entscheidung

Ein generischer **`SeedRunner`** führt geordnete **`ISeedStep`s** aus (`Id`, `Version`, `Order`). Der
Zustand liegt in `umbracoKeyValue`. Das Inhaltsmodell ist deklarativ (`*Definitions.cs`).

## Konsequenzen

- Positiv: reproduzierbare Umgebungen, Schema-Änderungen per Code-Review nachvollziehbar.
- Negativ: Änderungen im Backoffice werden nicht zurück in den Code synchronisiert. Regel: Schema nur per Code ändern.
- Alternative für später: uSync bzw. Umbraco Deploy, falls Redaktionen Schema-Änderungen in Umgebungen übertragen müssen.
