using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using UmbracoAI.Web.Geo;
using UmbracoAI.Web.Site;

namespace UmbracoAI.Web.Seo;

/// <summary>Services for SEO meta data, structured data, sitemap/robots and the GEO (llms.txt) endpoints.</summary>
public sealed class SeoComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services.Configure<SiteOptions>(builder.Config.GetSection(SiteOptions.SectionName));
        builder.Services.AddSingleton<StructuredDataBuilder>();
        builder.Services.AddSingleton<PageSeoService>();
        builder.Services.AddSingleton<AgentMarkdownService>();
        builder.Services.AddScoped<SiteResolver>();
    }
}
