namespace UmbracoAI.Web.Seeding.Mcp;

/// <summary>
/// Client credentials of the API user the Umbraco Developer MCP server (@umbraco-cms/mcp-dev) signs in with.
/// The user is only created when a secret is configured.
/// </summary>
public sealed class McpApiUserOptions
{
    public const string SectionName = "McpApiUser";

    /// <summary>Umbraco requires back-office client ids to start with this prefix.</summary>
    public const string ClientIdPrefix = "umbraco-back-office-";

    public string ClientId { get; set; } = ClientIdPrefix + "mcp";

    public string? ClientSecret { get; set; }

    public string Name { get; set; } = "Umbraco MCP";

    public string Email { get; set; } = "mcp@starter.local";
}
