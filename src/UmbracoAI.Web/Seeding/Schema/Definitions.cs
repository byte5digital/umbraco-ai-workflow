namespace UmbracoAI.Web.Seeding.Schema;

/// <summary>Declarative property of a content type – resolved against the data types by name.</summary>
public sealed record PropertyDef(
    string Alias,
    string Name,
    string DataTypeName,
    bool Mandatory = false,
    string? Description = null,
    bool VariesByCulture = false);

/// <summary>An element type (Block Grid module, Block List item or block settings).</summary>
public sealed record ElementDef(
    string Alias,
    string Name,
    string Icon,
    IReadOnlyList<PropertyDef> Properties,
    string? Description = null,
    bool VariesByCulture = false);

/// <summary>A backoffice tab of a document type (Inhalt, SEO, Social Media, GEO & KI …).</summary>
public sealed record TabDef(string Alias, string Name, int SortOrder);

/// <summary>A group (box) inside a tab. Umbraco addresses nested groups as "tabAlias/groupKey".</summary>
public sealed record GroupDef(TabDef Tab, string Key, string Name, int SortOrder, IReadOnlyList<PropertyDef> Properties)
{
    public string Alias => $"{Tab.Alias}/{Key}";
}

/// <summary>A page document type: compositions, tab groups, template and allowed children.</summary>
public sealed record PageTypeDef(
    string Alias,
    string Name,
    string Icon,
    IReadOnlyList<GroupDef> Groups,
    IReadOnlyList<string> AllowedChildren,
    bool IsRoot = false,
    bool AllowSelfAsChild = false);
