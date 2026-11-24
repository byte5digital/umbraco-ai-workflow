namespace Opten.Web.Seeding.Schema;

/// <summary>Builds the configuration of the "Inhaltsmodule" Block Grid data type.</summary>
public static class BlockGridConfigFactory
{
    private const int GridColumns = 12;

    private static readonly Guid GroupStage = new("8b0f6a52-2a3e-4b8e-9d0e-6c1a7f3e2b01");
    private static readonly Guid GroupContent = new("8b0f6a52-2a3e-4b8e-9d0e-6c1a7f3e2b02");
    private static readonly Guid GroupTeaser = new("8b0f6a52-2a3e-4b8e-9d0e-6c1a7f3e2b03");

    private static readonly IReadOnlyDictionary<string, Guid> GroupByModule = new Dictionary<string, Guid>
    {
        [Elements.HeroStage] = GroupStage,
        [Elements.RichText] = GroupContent,
        [Elements.ImageText] = GroupContent,
        [Elements.FaqAccordion] = GroupContent,
        [Elements.TeaserSection] = GroupTeaser,
        [Elements.CtaBanner] = GroupTeaser,
        [Elements.NewsList] = GroupTeaser,
    };

    public static Dictionary<string, object> Create(IReadOnlyDictionary<string, Guid> elementKeys)
    {
        Guid settingsKey = elementKeys[Elements.SectionSettings];
        object[] blocks = ElementDefinitions.Modules.Select(module => (object)CreateBlock(module, elementKeys, settingsKey)).ToArray();

        return new Dictionary<string, object>
        {
            ["gridColumns"] = GridColumns,
            ["blocks"] = blocks,
            ["blockGroups"] = new object[]
            {
                Group(GroupStage, "Bühne"),
                Group(GroupContent, "Inhalt"),
                Group(GroupTeaser, "Teaser & Listen"),
            },
            ["validationLimit"] = new Dictionary<string, object?> { ["min"] = 0, ["max"] = null },
            ["useLiveEditing"] = true,
        };
    }

    private static Dictionary<string, object?> CreateBlock(ElementDef module, IReadOnlyDictionary<string, Guid> elementKeys, Guid settingsKey) => new()
    {
        ["contentElementTypeKey"] = elementKeys[module.Alias],
        ["settingsElementTypeKey"] = settingsKey,
        ["label"] = module.Properties.Any(p => p.Alias == Props.Headline) ? $"{module.Name} – {{umbValue: headline}}" : module.Name,
        ["groupKey"] = GroupByModule.TryGetValue(module.Alias, out Guid group) ? group : GroupContent,
        ["allowAtRoot"] = true,
        ["allowInAreas"] = false,
        ["columnSpanOptions"] = new[] { new Dictionary<string, object> { ["columnSpan"] = GridColumns } },
        ["rowMinSpan"] = 1,
        ["rowMaxSpan"] = 1,
        ["editorSize"] = "large",
        ["forceHideContentEditorInOverlay"] = false,
    };

    private static Dictionary<string, object> Group(Guid key, string name) => new() { ["key"] = key, ["name"] = name };
}
