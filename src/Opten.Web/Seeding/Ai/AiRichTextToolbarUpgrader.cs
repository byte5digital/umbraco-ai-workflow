using System.Text.Json.Nodes;
using Opten.Web.Seeding.Schema;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;

namespace Opten.Web.Seeding.Ai;

/// <summary>
/// Adds the Umbraco AI prompt button to the toolbar of the "Richtext editor" data type, so the rich text prompts
/// (improve, plain language, translate) are available on the selected text. Idempotent.
/// </summary>
public sealed class AiRichTextToolbarUpgrader(
    IDataTypeService dataTypeService,
    IConfigurationEditorJsonSerializer configurationSerializer,
    ILogger<AiRichTextToolbarUpgrader> logger)
{
    private const string ExtensionsKey = "extensions";
    private const string ToolbarKey = "toolbar";

    public async Task UpgradeAsync()
    {
        IDataType richText = (await dataTypeService.GetAllAsync()).FirstOrDefault(dt => dt.Name == DataTypeNames.RichText)
            ?? throw new InvalidOperationException($"Data type '{DataTypeNames.RichText}' not found.");

        JsonObject configuration = JsonNode.Parse(configurationSerializer.Serialize(richText.ConfigurationData))?.AsObject()
            ?? new JsonObject();

        bool hasExtension = AddExtension(configuration);
        bool hasToolbarItem = AddToolbarItem(configuration);
        if (!hasExtension && !hasToolbarItem)
        {
            return;
        }

        richText.ConfigurationData = configurationSerializer.Deserialize<Dictionary<string, object>>(configuration.ToJsonString())
            ?? throw new InvalidOperationException($"Invalid configuration of '{richText.Name}'.");
        var result = await dataTypeService.UpdateAsync(richText, Constants.Security.SuperUserKey);
        if (!result.Success)
        {
            throw new InvalidOperationException($"Updating '{richText.Name}' failed: {result.Status}");
        }

        logger.LogInformation("AI setup: added AI prompts to the toolbar of {DataType}", richText.Name);
    }

    /// <returns>true when the extension was added.</returns>
    private static bool AddExtension(JsonObject configuration)
    {
        JsonArray extensions = configuration[ExtensionsKey] as JsonArray ?? [];
        if (extensions.Any(node => node?.GetValue<string>() == AiAliases.TiptapExtension))
        {
            return false;
        }

        configuration[ExtensionsKey] = new JsonArray([.. extensions.Select(node => node?.DeepClone()), AiAliases.TiptapExtension]);
        return true;
    }

    /// <summary>Toolbar shape: rows › groups › item aliases. The AI button becomes its own group at the end of row 1.</summary>
    /// <returns>true when the toolbar item was added.</returns>
    private static bool AddToolbarItem(JsonObject configuration)
    {
        JsonArray rows = configuration[ToolbarKey] as JsonArray ?? [];
        if (rows.ToJsonString().Contains(AiAliases.TiptapToolbarItem, StringComparison.Ordinal))
        {
            return false;
        }

        JsonArray firstRow = rows.Count > 0 && rows[0] is JsonArray row ? (JsonArray)row.DeepClone() : [];
        firstRow.Add(new JsonArray(AiAliases.TiptapToolbarItem));

        configuration[ToolbarKey] = new JsonArray([firstRow, .. rows.Skip(1).Select(node => node?.DeepClone())]);
        return true;
    }
}
