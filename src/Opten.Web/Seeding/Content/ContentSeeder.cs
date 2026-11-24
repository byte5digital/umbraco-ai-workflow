using Opten.Web.Seeding.Schema;
using Opten.Web.Site;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Opten.Web.Seeding.Content;

/// <summary>
/// Creates the page tree of <see cref="StarterContent.Pages"/> in all cultures, fills properties and Block Grids,
/// publishes everything and assigns the culture domains (/de, /en) to the start page.
/// </summary>
public sealed class ContentSeeder(
    IContentService contentService,
    IDomainService domainService,
    IJsonSerializer jsonSerializer,
    MediaSeeder mediaSeeder,
    BlockValueBuilder blockValueBuilder,
    ILogger<ContentSeeder> logger)
{
    private static readonly string[] SiteCultures = Cultures.All.Select(c => c.IsoCode).ToArray();

    public async Task CreateAsync()
    {
        IReadOnlyDictionary<string, IMedia> media = mediaSeeder.ImportAll();

        // 1) Create the tree first, so pages can link to each other by key.
        var nodes = new Dictionary<string, IContent>();
        foreach (PageSeed page in StarterContent.Pages)
        {
            int parentId = page.ParentId is null ? Constants.System.Root : nodes[page.ParentId].Id;
            nodes[page.Id] = CreateNode(page, parentId);
        }

        var pageKeys = nodes.ToDictionary(n => n.Key, n => n.Value.Key);

        // 2) Fill every culture, 3) publish parents before children (dictionary keeps insertion order).
        foreach (PageSeed page in StarterContent.Pages)
        {
            IContent node = nodes[page.Id];
            foreach (SiteCulture culture in Cultures.All)
            {
                var context = new SeedContext(media, pageKeys, blockValueBuilder, culture);
                SetValue(node, Props.MainContent, blockValueBuilder.Grid(page.Modules(context)), culture.IsoCode);
                foreach ((string alias, object? value) in page.Values(context))
                {
                    SetValue(node, alias, value, culture.IsoCode);
                }
            }

            SaveAndPublish(node);
        }

        await AssignDomainsAsync(nodes[StarterContent.Home].Key);
    }

    private IContent CreateNode(PageSeed page, int parentId)
    {
        IContent node = contentService.Create(page.Name.German, parentId, page.DocType);
        foreach (SiteCulture culture in Cultures.All)
        {
            node.SetCultureName(page.Name.For(culture), culture.IsoCode);
        }

        Save(node);
        return node;
    }

    /// <summary>Sets a value for the culture – or invariantly when the property does not vary by culture.</summary>
    private void SetValue(IContent node, string alias, object? value, string culture)
    {
        if (value is null)
        {
            return;
        }

        if (!node.Properties.TryGetValue(alias, out IProperty? property))
        {
            throw new InvalidOperationException($"Document type '{node.ContentType.Alias}' has no property '{alias}'.");
        }

        node.SetValue(alias, Serialize(value), property.PropertyType.VariesByCulture() ? culture : null);
    }

    private object Serialize(object value) => value switch
    {
        string or DateTime or bool or int => value,
        _ => jsonSerializer.Serialize(value),
    };

    private void Save(IContent node)
    {
        var result = contentService.Save(node);
        SeedGuard.Ensure(result.Success, $"saving '{node.Name}'", result.Result);
    }

    private void SaveAndPublish(IContent node)
    {
        Save(node);
        var result = contentService.Publish(node, SiteCultures);
        SeedGuard.Ensure(result.Success, $"publishing '{node.Name}'", result.Result);
        logger.LogInformation("Seed: published {PageName} ({Cultures})", node.Name, string.Join(", ", SiteCultures));
    }

    private async Task AssignDomainsAsync(Guid homeKey)
    {
        var update = new DomainsUpdateModel
        {
            DefaultIsoCode = Cultures.Default.IsoCode,
            Domains = Cultures.All
                .Select(culture => new DomainModel { DomainName = culture.DomainPath, IsoCode = culture.IsoCode })
                .ToArray(),
        };

        var result = await domainService.UpdateDomainsAsync(homeKey, update);
        SeedGuard.Ensure(result.Success, "assigning culture domains", result.Status);
    }
}
