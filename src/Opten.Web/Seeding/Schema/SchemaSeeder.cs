using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using static Opten.Web.Seeding.Schema.SeoGeoDefinitions;

namespace Opten.Web.Seeding.Schema;

/// <summary>
/// Creates the complete content model in dependency order:
/// basic data types → list items → block lists → settings + modules → block grid → compositions → templates → page types.
/// </summary>
public sealed class SchemaSeeder(
    DataTypeSeeder dataTypeSeeder,
    ContentTypeFactory factory,
    IContentTypeService contentTypeService,
    ITemplateService templateService,
    IWebHostEnvironment hostingEnvironment)
{
    private const string MasterTemplate = "master";

    public async Task CreateAsync()
    {
        await dataTypeSeeder.CreateBasicTypesAsync();
        await RefreshDataTypesAsync();

        var elementKeys = new Dictionary<string, Guid>();
        await CreateElementsAsync(ElementDefinitions.ListItems, elementKeys);
        await dataTypeSeeder.CreateBlockListTypesAsync(elementKeys);
        await RefreshDataTypesAsync();

        await CreateElementsAsync([ElementDefinitions.SectionSettings, .. ElementDefinitions.Modules], elementKeys);
        await dataTypeSeeder.CreateBlockGridAsync(elementKeys);
        await RefreshDataTypesAsync();

        IContentType seo = await CreateCompositionAsync(DocTypes.SeoComposition, "SEO (Komposition)", "icon-search", SeoGroups);
        IContentType geo = await CreateCompositionAsync(DocTypes.GeoComposition, "GEO & KI (Komposition)", "icon-lightbulb-active", GeoGroups);

        // Page templates use "master.cshtml" as layout, so the master template must exist first.
        await EnsureTemplateAsync(MasterTemplate, "Master");
        foreach (PageTypeDef definition in PageTypeDefinitions.All)
        {
            await CreatePageTypeAsync(definition, [seo, geo]);
        }
    }

    private async Task CreateElementsAsync(IEnumerable<ElementDef> definitions, IDictionary<string, Guid> keys)
    {
        foreach (ElementDef definition in definitions)
        {
            IContentType element = contentTypeService.Get(definition.Alias) ?? await factory.CreateElementAsync(definition);
            keys[definition.Alias] = element.Key;
        }
    }

    private async Task<IContentType> CreateCompositionAsync(string alias, string name, string icon, IReadOnlyList<GroupDef> groups)
    {
        if (contentTypeService.Get(alias) is { } existing)
        {
            // Existing installations: add new tabs/groups/properties, move existing ones (values are kept).
            TabLayout.Apply(existing, groups, factory);
            await factory.UpdateAsync(existing);
            return existing;
        }

        var composition = factory.Build(alias, name, icon, isElement: false, variesByCulture: true);
        TabLayout.Apply(composition, groups, factory);
        return await factory.CreateAsync(composition);
    }

    private async Task CreatePageTypeAsync(PageTypeDef definition, IReadOnlyList<IContentType> compositions)
    {
        if (contentTypeService.Get(definition.Alias) is not null)
        {
            return;
        }

        // All pages vary by culture; invariant properties (images, dates, flags) are shared between languages.
        var docType = factory.Build(definition.Alias, definition.Name, definition.Icon, isElement: false, variesByCulture: true);
        docType.AllowedAsRoot = definition.IsRoot;
        foreach (IContentType composition in compositions)
        {
            docType.AddContentType(composition);
        }

        TabLayout.Apply(docType, definition.Groups, factory);

        ITemplate template = await EnsureTemplateAsync(definition.Alias, definition.Name);
        docType.AllowedTemplates = [template];
        docType.SetDefaultTemplate(template);
        docType.AllowedContentTypes = definition.AllowedChildren
            .Select((alias, index) => contentTypeService.Get(alias)
                ?? throw new InvalidOperationException($"Child type '{alias}' of '{definition.Alias}' must be defined first."))
            .Select((child, index) => new ContentTypeSort(child.Key, index, child.Alias))
            .ToArray();

        IContentType saved = await factory.CreateAsync(docType);
        if (definition.AllowSelfAsChild)
        {
            saved.AllowedContentTypes = [.. saved.AllowedContentTypes ?? [], new ContentTypeSort(saved.Key, 99, saved.Alias)];
            await factory.UpdateAsync(saved);
        }
    }

    /// <summary>
    /// Registers the template in Umbraco. The view file ships with the project, so its content is passed through –
    /// Umbraco would otherwise overwrite the file with an empty template (learning from dus-umbraco).
    /// </summary>
    private async Task<ITemplate> EnsureTemplateAsync(string alias, string name)
    {
        if (await templateService.GetAsync(alias) is { } existing)
        {
            return existing;
        }

        string viewPath = Path.Combine(hostingEnvironment.ContentRootPath, "Views", $"{alias}.cshtml");
        string? content = System.IO.File.Exists(viewPath) ? await System.IO.File.ReadAllTextAsync(viewPath) : null;

        var result = await templateService.CreateAsync(name, alias, content, Constants.Security.SuperUserKey);
        if (!result.Success || result.Result is null)
        {
            throw new InvalidOperationException($"Creating template '{alias}' failed: {result.Status}");
        }

        return result.Result;
    }

    private async Task RefreshDataTypesAsync() => factory.DataTypes = await dataTypeSeeder.GetAllByNameAsync();
}
