using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Umbraco.AI.Search.Startup;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Web.Common.ApplicationBuilder;
using UmbracoAI.Web.Seo;
using UmbracoAI.Web.Site;

namespace UmbracoAI.Web.Search;

/// <summary>Site search services, the rate limiter and the JSON endpoint GET /api/search?q=…&amp;culture=de-DE.</summary>
[ComposeAfter(typeof(UmbracoAISearchComposer))]
public sealed class SearchComposer : IComposer
{
    public const string RateLimitPolicy = "site-search";
    public const string Route = "/api/search";

    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services.Configure<SiteSearchOptions>(builder.Config.GetSection(SiteSearchOptions.SectionName));
        builder.Services.AddMemoryCache();
        builder.Services.AddScoped<ContentSearchService>();
        builder.Services.AddScoped<SiteSearchService>();

        SiteSearchOptions settings = builder.Config.GetSection(SiteSearchOptions.SectionName).Get<SiteSearchOptions>() ?? new();
        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy(RateLimitPolicy, context => RateLimitPartition.GetFixedWindowLimiter(
                context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions { PermitLimit = settings.RequestsPerMinute, Window = TimeSpan.FromMinutes(1) }));
        });

        // The rate limiter must run after routing (it reads the endpoint's policy).
        builder.Services.Configure<UmbracoPipelineOptions>(options =>
            options.AddFilter(new UmbracoPipelineFilter(nameof(SearchComposer)) { PostRouting = app => app.UseRateLimiter() }));
    }
}

public static class SearchEndpoints
{
    public static void MapSearchEndpoints(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapGet(SearchComposer.Route, async (string? q, string? culture, HttpContext context, SiteResolver resolver, SiteSearchService search) =>
        {
            string query = SiteSearchOptions.Normalize(q);
            if (query.Length < SiteSearchOptions.MinQueryLength)
            {
                return Results.BadRequest(new { error = $"The query must have at least {SiteSearchOptions.MinQueryLength} characters." });
            }

            SiteContext? site = resolver.Resolve(context.Request, Cultures.Get(culture).IsoCode);
            return site is null
                ? Results.Problem("The site is not published yet.", statusCode: StatusCodes.Status503ServiceUnavailable)
                : Results.Json(await search.SearchAsync(query, site));
        }).RequireRateLimiting(SearchComposer.RateLimitPolicy);
}
