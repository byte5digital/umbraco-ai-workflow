# Seeding (Seeder-Template)

Beim Start führt `SeedRunner` alle registrierten `ISeedStep`s in der Reihenfolge `Order` aus. Erfolgreiche Steps
speichern ihre `Version` in der Umbraco-Tabelle `umbracoKeyValue` (Schlüssel `Starterkit.Seed.{Id}`) und laufen danach nicht erneut.

| Order | Step | Zweck |
| --- | --- | --- |
| 100 | `languages` | Sprachen aus `Site/Cultures.cs` |
| 200 | `schema` | Datentypen, Elemente, Kompositionen (SEO, GEO), Templates, Seitentypen |
| 300 | `dictionary` | UI-Texte aus `Site/UiTexts.cs` |
| 400 | `starter-content` | Zweisprachige Starter-Seiten (nur bei leerem Inhaltsbaum) |
| 500 | `umbraco-ai` | Umbraco.AI-Setup (wartet auf API-Key) |
| 600 | `umbraco-ai-search` | Embedding-Profil + Vektorindex befüllen |

`Completed` = erledigt, `Skipped` = Voraussetzung fehlt (z. B. API-Key), wird beim nächsten Start erneut versucht.
Eine Exception stoppt die Kette, die Seite läuft trotzdem weiter.

## Inhaltsmodell ändern

1. Definition anpassen: `Seeding/Schema/ElementDefinitions.cs`, `PageTypeDefinitions.cs` oder `SeoGeoDefinitions.cs`.
2. **Frische Installation:** genügt (Datenbank löschen, neu starten).
3. **Bestehende Installation:** neuen Step anlegen, z. B. `Steps/AddEventPageStep.cs` mit `Order = 700`.
   `TabLayout.Apply` ist idempotent: Es legt fehlende Tabs und Properties an und verschiebt bestehende, ohne Werte
   zu verlieren. Bei Änderungen an einem bestehenden Step dessen `Version` erhöhen.

```csharp
public sealed class AddEventPageStep(ContentTypeFactory factory, DataTypeSeeder dataTypes, IContentTypeService types) : ISeedStep
{
    public string Id => "event-page";
    public int Version => 1;
    public int Order => 700;

    public async Task<SeedStepResult> ExecuteAsync(CancellationToken cancellationToken)
    {
        factory.DataTypes = await dataTypes.GetAllByNameAsync();
        IContentType page = types.Get(DocTypes.ContentPage)!;
        TabLayout.Apply(page, [new GroupDef(SeoGeoDefinitions.ContentTab, "event", "Event", 1,
            [new(Props.PublishDate, "Datum", DataTypeNames.DatePicker)])], factory);
        await factory.UpdateAsync(page);
        return SeedStepResult.Completed;
    }
}
```

## Starter-Inhalte

`Seeding/Content/StarterContent.cs` beschreibt Seiten als `PageSeed` (Id, Seitentyp, Name DE/EN, Eltern-Id, Module,
Property-Werte). Bilder stammen aus `SeedData/images/` und werden per Dateiname ohne Endung referenziert
(`c.Media("hero")`). Fehlt ein Bild, entfällt es bzw. das Modul fällt auf Fließtext zurück.

Starter-Inhalte abschalten: `Seed__StarterContent=false`. Dann wird nur das Inhaltsmodell angelegt.

## Learnings aus früheren Projekten

- Templates **mit Inhalt** registrieren, sonst überschreibt Umbraco die View-Datei mit einem leeren Template.
- Block-Werte im Speicherformat des Backoffice schreiben: Strukturen als JSON-String, verschachtelte Blöcke als Objekt
  (`BlockValueBuilder`).
- Seit Umbraco 14 braucht jeder Datentyp einen `EditorUiAlias`.
- Content, der vor dem Embedding-Profil veröffentlicht wurde, landet nicht im Vektorindex. Deshalb baut
  `AiSearchStep` den Index neu auf, wenn er leer ist.
