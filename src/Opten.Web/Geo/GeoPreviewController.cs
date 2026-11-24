using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Opten.Web.Seo;
using Opten.Web.Site;
using Umbraco.Cms.Api.Management.Controllers;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Opten.Web.Geo;

/// <summary>
/// Data of the "llms.txt & Markdown" preview in the GEO tab (GET /umbraco/management/api/v1/opten/geo/page/{key}).
/// Renders the latest saved version (draft) of the page; requires a backoffice token with access to the content section.
/// </summary>
[ApiVersion("1.0")]
[VersionedApiBackOfficeRoute("opten/geo")]
[ApiExplorerSettings(IgnoreApi = true)]
[Authorize(Policy = AuthorizationPolicies.SectionAccessContent)]
public sealed class GeoPreviewController(SiteResolver siteResolver, AgentMarkdownService markdown) : ManagementApiControllerBase
{
    /// <summary>Rough token estimate for typical LLM tokenizers (≈ 4 characters per token).</summary>
    private const int CharsPerToken = 4;

    [HttpGet("page/{key:guid}")]
    [ProducesResponseType<GeoPagePreview>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Page(Guid key, string? culture)
    {
        string isoCode = Cultures.Get(culture).IsoCode;
        if (siteResolver.ResolveDraft(Request, key, isoCode) is not { } resolved)
        {
            return NotFound();
        }

        (IPublishedContent page, SiteContext site) = resolved;
        bool isPublished = page.IsPublished(isoCode);
        string pageMarkdown = markdown.BuildPage(page, site);

        return Ok(new GeoPagePreview(
            isoCode,
            isPublished,
            isPublished ? site.PageUrl(page) : string.Empty,
            isPublished ? site.MarkdownUrl(page) : string.Empty,
            site.LlmsUrl(full: false),
            site.LlmsUrl(full: true),
            page.Description(isoCode),
            page.AgentExclusionReason(isoCode),
            page.IsHome() ? null : AgentMarkdownService.IndexEntry(page, site),
            pageMarkdown,
            pageMarkdown.Length / CharsPerToken));
    }
}

/// <param name="FallbackDescription">Description used when neither KI-Zusammenfassung nor meta description is set.</param>
/// <param name="ExclusionReason">"noindex" or "excluded" when the page is hidden from llms.txt, otherwise null.</param>
/// <param name="LlmsEntry">The line of this page in llms.txt (null for the start page, which forms the header).</param>
public sealed record GeoPagePreview(
    string Culture,
    bool IsPublished,
    string PageUrl,
    string MarkdownUrl,
    string LlmsUrl,
    string LlmsFullUrl,
    string? FallbackDescription,
    string? ExclusionReason,
    string? LlmsEntry,
    string Markdown,
    int EstimatedTokens);
