namespace Opten.Web.Seeding.Content;

/// <summary>
/// One page of the starter site. <see cref="Values"/> returns all property values for a culture (SEO, GEO, site
/// settings …) – invariant properties are written once, culture variants per language automatically.
/// </summary>
/// <param name="Id">Seed-internal id used for links between pages ("about", "news" …).</param>
/// <param name="ParentId">Null = root (the start page).</param>
public sealed record PageSeed(
    string Id,
    string DocType,
    Bilingual Name,
    string? ParentId,
    Func<SeedContext, IReadOnlyList<Block>> Modules,
    Func<SeedContext, IReadOnlyDictionary<string, object?>> Values);
