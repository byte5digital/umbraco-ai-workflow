using Opten.Web.Configuration;
using Opten.Web.Geo;
using Opten.Web.Search;

// Local development without Docker: values from .env (repo root) become environment variables.
// Already-set variables (shell, CI, container) always win.
DotEnvFile.Load(DotEnvFile.Find(Directory.GetCurrentDirectory()));

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

#if DEBUG
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);
#endif

// Razor rendering only – no Content Delivery API (see docs/adr/0002-razor-ohne-delivery-api.md).
builder.CreateUmbracoBuilder()
    .AddBackOffice()
    .AddWebsite()
    .AddComposers()
    .Build();

WebApplication app = builder.Build();

await app.BootUmbracoAsync();

app.UseUmbraco()
    .WithMiddleware(u =>
    {
        u.UseBackOffice();
        u.UseWebsite();
    })
    .WithEndpoints(u =>
    {
        // "/" redirect, robots.txt, sitemap.xml, llms.txt, llms-full.txt and the Markdown twin (.md) of every page
        u.EndpointRouteBuilder.MapAgenticEndpoints();
        u.EndpointRouteBuilder.MapSearchEndpoints();
        u.UseBackOfficeEndpoints();
        u.UseWebsiteEndpoints();
    });

await app.RunAsync();
