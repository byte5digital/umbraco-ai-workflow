using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;

namespace Opten.Web.Seeding.Schema;

/// <summary>Low-level helper that builds and persists content types from <see cref="PropertyDef"/> lists.</summary>
public sealed class ContentTypeFactory(IContentTypeService contentTypeService, IShortStringHelper shortStringHelper)
{
    private static readonly Guid UserKey = Constants.Security.SuperUserKey;

    /// <summary>All data types by name – refresh after creating data types (see <see cref="SchemaSeeder"/>).</summary>
    public IReadOnlyDictionary<string, IDataType> DataTypes { get; set; } = new Dictionary<string, IDataType>();

    public ContentType Build(
        string alias, string name, string icon, bool isElement, string? description = null, bool variesByCulture = false) =>
        new(shortStringHelper, Constants.System.Root)
        {
            Alias = alias,
            Name = name,
            Icon = icon,
            IsElement = isElement,
            Description = description,
            Variations = variesByCulture ? ContentVariation.Culture : ContentVariation.Nothing,
        };

    public async Task<IContentType> CreateElementAsync(ElementDef definition)
    {
        ContentType element = Build(
            definition.Alias, definition.Name, definition.Icon, isElement: true, definition.Description, definition.VariesByCulture);
        TabLayout.Apply(element, [new GroupDef(SeoGeoDefinitions.ContentTab, "main", "Inhalt", 0, definition.Properties)], this);
        return await CreateAsync(element);
    }

    public async Task<IContentType> CreateAsync(ContentType contentType)
    {
        var result = await contentTypeService.CreateAsync(contentType, UserKey);
        if (!result.Success)
        {
            throw new InvalidOperationException($"Creating content type '{contentType.Alias}' failed: {result.Result}");
        }

        return contentType;
    }

    public async Task UpdateAsync(IContentType contentType)
    {
        var result = await contentTypeService.UpdateAsync(contentType, UserKey);
        if (!result.Success)
        {
            throw new InvalidOperationException($"Updating content type '{contentType.Alias}' failed: {result.Result}");
        }
    }

    public PropertyType CreatePropertyType(PropertyDef property)
    {
        if (!DataTypes.TryGetValue(property.DataTypeName, out IDataType? dataType))
        {
            string available = string.Join(", ", DataTypes.Keys.Order());
            throw new InvalidOperationException(
                $"Data type '{property.DataTypeName}' for property '{property.Alias}' not found. Available: {available}");
        }

        return new PropertyType(shortStringHelper, dataType, property.Alias)
        {
            Name = property.Name,
            Description = property.Description,
            Mandatory = property.Mandatory,
            Variations = property.VariesByCulture ? ContentVariation.Culture : ContentVariation.Nothing,
        };
    }
}
