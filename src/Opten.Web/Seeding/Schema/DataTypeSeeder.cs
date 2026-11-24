using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;

namespace Opten.Web.Seeding.Schema;

/// <summary>Creates the custom data types (links, dropdowns, block lists, block grid). Idempotent by name.</summary>
public sealed class DataTypeSeeder(
    IDataTypeService dataTypeService,
    PropertyEditorCollection propertyEditors,
    IConfigurationEditorJsonSerializer configurationSerializer,
    ILogger<DataTypeSeeder> logger)
{
    private static readonly Guid UserKey = Constants.Security.SuperUserKey;

    /// <summary>Backoffice UI for each server-side property editor (required since Umbraco 14).</summary>
    private static readonly IReadOnlyDictionary<string, string> EditorUiAliases = new Dictionary<string, string>
    {
        [Constants.PropertyEditors.Aliases.MultiUrlPicker] = "Umb.PropertyEditorUi.MultiUrlPicker",
        [Constants.PropertyEditors.Aliases.DropDownListFlexible] = "Umb.PropertyEditorUi.Dropdown",
        [Constants.PropertyEditors.Aliases.BlockList] = "Umb.PropertyEditorUi.BlockList",
        [Constants.PropertyEditors.Aliases.BlockGrid] = "Umb.PropertyEditorUi.BlockGrid",
    };

    /// <summary>Custom backoffice UI (wwwroot/App_Plugins/OptenGeo) – display only, stored as an empty label.</summary>
    public const string LlmsPreviewUi = "Opten.PropertyEditorUi.LlmsPreview";

    public async Task CreateBasicTypesAsync()
    {
        await EnsureAsync(DataTypeNames.SingleLink, Constants.PropertyEditors.Aliases.MultiUrlPicker,
            new() { ["minNumber"] = 0, ["maxNumber"] = 1 });
        await EnsureAsync(DataTypeNames.MultiLinks, Constants.PropertyEditors.Aliases.MultiUrlPicker,
            new() { ["minNumber"] = 0, ["maxNumber"] = 0 });
        await EnsureDropdownAsync(DataTypeNames.Background, Options.Backgrounds);
        await EnsureDropdownAsync(DataTypeNames.ImagePosition, Options.ImagePositions);
        await EnsureDropdownAsync(DataTypeNames.SitemapChangeFrequency, Options.SitemapChangeFrequencies);
        await EnsureDropdownAsync(DataTypeNames.SitemapPriority, Options.SitemapPriorities);
        await EnsureDropdownAsync(DataTypeNames.SchemaType, Options.SchemaTypes);
        await EnsureAsync(DataTypeNames.LlmsPreview, Constants.PropertyEditors.Aliases.Label,
            new() { ["umbracoDataValueType"] = "STRING" }, LlmsPreviewUi);
    }

    public async Task CreateBlockListTypesAsync(IReadOnlyDictionary<string, Guid> elementKeys)
    {
        await EnsureBlockListAsync(DataTypeNames.TeaserCardList, elementKeys[Elements.TeaserCard], "{umbValue: headline}");
        await EnsureBlockListAsync(DataTypeNames.FaqItemList, elementKeys[Elements.FaqItem], "{umbValue: question}");
    }

    public Task CreateBlockGridAsync(IReadOnlyDictionary<string, Guid> elementKeys) =>
        EnsureAsync(DataTypeNames.ContentGrid, Constants.PropertyEditors.Aliases.BlockGrid, BlockGridConfigFactory.Create(elementKeys));

    /// <summary>Returns all data types indexed by name (built-in + custom).</summary>
    public async Task<IReadOnlyDictionary<string, IDataType>> GetAllByNameAsync()
    {
        IEnumerable<IDataType> all = await dataTypeService.GetAllAsync();
        return all
            .Where(dt => dt.Name is not null)
            .GroupBy(dt => dt.Name!)
            .ToDictionary(g => g.Key, g => g.First());
    }

    private Task EnsureDropdownAsync(string name, IEnumerable<string> items) =>
        EnsureAsync(name, Constants.PropertyEditors.Aliases.DropDownListFlexible,
            new() { ["multiple"] = false, ["items"] = items.ToArray() });

    private Task EnsureBlockListAsync(string name, Guid elementKey, string label) =>
        EnsureAsync(name, Constants.PropertyEditors.Aliases.BlockList, new()
        {
            ["blocks"] = new object[]
            {
                new Dictionary<string, object?>
                {
                    ["contentElementTypeKey"] = elementKey,
                    ["label"] = label,
                    ["editorSize"] = "medium",
                },
            },
            ["validationLimit"] = new Dictionary<string, object?> { ["min"] = 0, ["max"] = null },
            ["useSingleBlockMode"] = false,
            ["useLiveEditing"] = false,
            ["useInlineEditingAsDefault"] = false,
        });

    private async Task EnsureAsync(
        string name, string editorAlias, Dictionary<string, object> configuration, string? editorUiAlias = null)
    {
        if ((await GetAllByNameAsync()).ContainsKey(name))
        {
            return;
        }

        if (!propertyEditors.TryGet(editorAlias, out IDataEditor? editor))
        {
            throw new InvalidOperationException($"Property editor '{editorAlias}' is not available.");
        }

        editorUiAlias ??= EditorUiAliases.GetValueOrDefault(editorAlias)
            ?? throw new InvalidOperationException($"No backoffice editor UI mapped for '{editorAlias}'.");

        var dataType = new DataType(editor, configurationSerializer)
        {
            Name = name,
            EditorUiAlias = editorUiAlias,
            DatabaseType = ValueStorageType.Ntext,
            ConfigurationData = configuration,
        };

        var result = await dataTypeService.CreateAsync(dataType, UserKey);
        if (!result.Success)
        {
            throw new InvalidOperationException($"Creating data type '{name}' failed: {result.Status}");
        }

        logger.LogInformation("Seed: created data type {DataTypeName}", name);
    }
}
