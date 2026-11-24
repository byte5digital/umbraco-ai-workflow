using Opten.Web.Seeding.Schema;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;

namespace Opten.Web.Seeding.Content;

/// <summary>A block instance to be serialized into a Block Grid or Block List value.</summary>
public sealed record Block(string ElementAlias, IReadOnlyDictionary<string, object?> Values)
{
    public IReadOnlyDictionary<string, object?>? Settings { get; init; }

    public int ColumnSpan { get; init; } = 12;
}

/// <summary>
/// Serializes <see cref="Block"/> lists into the storage format of the Block Grid / Block List editors
/// (layout / contentData / settingsData / expose).
/// </summary>
public sealed class BlockValueBuilder(IContentTypeService contentTypeService, IJsonSerializer jsonSerializer)
{
    private const string BlockGridLayoutAlias = "Umbraco.BlockGrid";
    private const string BlockListLayoutAlias = "Umbraco.BlockList";

    public Dictionary<string, object> Grid(IEnumerable<Block> blocks)
    {
        var data = new BlockData();
        object[] layout = blocks.Select(block =>
        {
            (Guid contentKey, Guid? settingsKey) = AddElement(block, data, Elements.SectionSettings);
            return (object)new Dictionary<string, object?>
            {
                ["contentKey"] = contentKey,
                ["settingsKey"] = settingsKey,
                ["columnSpan"] = block.ColumnSpan,
                ["rowSpan"] = 1,
                ["areas"] = Array.Empty<object>(),
            };
        }).ToArray();
        return data.ToValue(BlockGridLayoutAlias, layout);
    }

    public Dictionary<string, object> List(IEnumerable<Block> blocks)
    {
        var data = new BlockData();
        object[] layout = blocks.Select(block =>
        {
            (Guid contentKey, _) = AddElement(block, data, settingsAlias: null);
            return (object)new Dictionary<string, object?> { ["contentKey"] = contentKey, ["settingsKey"] = null };
        }).ToArray();
        return data.ToValue(BlockListLayoutAlias, layout);
    }

    private (Guid ContentKey, Guid? SettingsKey) AddElement(Block block, BlockData data, string? settingsAlias)
    {
        Guid contentKey = Guid.NewGuid();
        data.Content.Add(ElementData(block.ElementAlias, contentKey, block.Values));
        data.Expose.Add(new Dictionary<string, object?> { ["contentKey"] = contentKey, ["culture"] = null, ["segment"] = null });

        if (settingsAlias is null || block.Settings is null)
        {
            return (contentKey, null);
        }

        Guid settingsKey = Guid.NewGuid();
        data.Settings.Add(ElementData(settingsAlias, settingsKey, block.Settings));
        return (contentKey, settingsKey);
    }

    private Dictionary<string, object> ElementData(string elementAlias, Guid key, IReadOnlyDictionary<string, object?> values)
    {
        IContentType elementType = contentTypeService.Get(elementAlias)
            ?? throw new InvalidOperationException($"Element type '{elementAlias}' does not exist.");
        var propertyTypes = elementType.CompositionPropertyTypes.ToDictionary(p => p.Alias);

        object[] serializedValues = values
            .Where(pair => pair.Value is not null)
            .Select(pair => (object)new Dictionary<string, object?>
            {
                ["alias"] = pair.Key,
                ["editorAlias"] = propertyTypes.TryGetValue(pair.Key, out IPropertyType? propertyType)
                    ? propertyType.PropertyEditorAlias
                    : throw new InvalidOperationException($"Element type '{elementAlias}' has no property '{pair.Key}'."),
                ["culture"] = null,
                ["segment"] = null,
                ["value"] = ToStorageValue(pair.Value),
            })
            .ToArray();

        return new Dictionary<string, object>
        {
            ["contentTypeKey"] = elementType.Key,
            ["key"] = key,
            ["values"] = serializedValues,
        };
    }

    /// <summary>
    /// Values inside blocks are stored the way the backoffice persists them: primitives as-is, structured values
    /// (pickers, dropdowns, rich text) as JSON strings. Nested block values stay objects.
    /// </summary>
    private object? ToStorageValue(object? value) => value switch
    {
        null or string or bool or int or long or decimal or double or DateTime => value,
        Dictionary<string, object> nested when nested.ContainsKey("layout") => nested,
        _ => jsonSerializer.Serialize(value),
    };

    private sealed class BlockData
    {
        public List<Dictionary<string, object>> Content { get; } = [];
        public List<Dictionary<string, object>> Settings { get; } = [];
        public List<Dictionary<string, object?>> Expose { get; } = [];

        public Dictionary<string, object> ToValue(string layoutAlias, object layout) => new()
        {
            ["layout"] = new Dictionary<string, object> { [layoutAlias] = layout },
            ["contentData"] = Content,
            ["settingsData"] = Settings,
            ["expose"] = Expose,
        };
    }
}
