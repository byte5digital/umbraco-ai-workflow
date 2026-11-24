using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;

namespace Opten.Web.Seeding.Content;

/// <summary>A link target in seed data – either an internal page (by key) or an external URL.</summary>
public sealed record LinkDef(string Name, string? Url = null, Guid? ContentKey = null, bool NewWindow = false)
{
    public static LinkDef External(string name, string url) => new(name, url, NewWindow: true);
}

/// <summary>Produces property values in the storage format expected by the built-in property editors.</summary>
public static class PropertyValues
{
    public static object[] Links(params LinkDef[] links) => links.Select(LinkDto).ToArray();

    public static object[] Link(LinkDef link) => [LinkDto(link)];

    public static object[]? Image(IMedia? media) => media is null
        ? null
        :
        [
            new Dictionary<string, object?>
            {
                ["key"] = Guid.NewGuid(),
                ["mediaKey"] = media.Key,
                ["mediaTypeAlias"] = media.ContentType.Alias,
                ["crops"] = Array.Empty<object>(),
                ["focalPoint"] = null,
            },
        ];

    public static object RichText(string markup) => new Dictionary<string, object?> { ["markup"] = markup, ["blocks"] = null };

    public static string[] Dropdown(string value) => [value];

    public static string DocumentUdi(Guid key) => Udi.Create(Constants.UdiEntityType.Document, key).ToString();

    private static Dictionary<string, object?> LinkDto(LinkDef link) => new()
    {
        ["name"] = link.Name,
        ["target"] = link.NewWindow ? "_blank" : null,
        ["udi"] = link.ContentKey is { } key ? DocumentUdi(key) : null,
        ["url"] = link.ContentKey is null ? link.Url : null,
        ["queryString"] = null,
    };
}
