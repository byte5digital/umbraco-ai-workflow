using Opten.Web.Seeding.Ai;
using Opten.Web.Seeding.Content;
using Opten.Web.Seeding.Mcp;
using Opten.Web.Seeding.Schema;
using Opten.Web.Seeding.Steps;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Notifications;

namespace Opten.Web.Seeding;

/// <summary>Registers the seed steps and their helpers. New step = implement <see cref="ISeedStep"/> + one line here.</summary>
public sealed class SeedComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services.Configure<SeedOptions>(builder.Config.GetSection(SeedOptions.SectionName));
        builder.Services.Configure<AiSetupOptions>(builder.Config.GetSection(AiSetupOptions.SectionName));
        builder.Services.Configure<McpApiUserOptions>(builder.Config.GetSection(McpApiUserOptions.SectionName));

        builder.Services.AddScoped<DataTypeSeeder>();
        builder.Services.AddScoped<ContentTypeFactory>();
        builder.Services.AddScoped<SchemaSeeder>();
        builder.Services.AddScoped<BlockValueBuilder>();
        builder.Services.AddScoped<MediaSeeder>();
        builder.Services.AddScoped<ContentSeeder>();
        builder.Services.AddScoped<AiSetupSeeder>();
        builder.Services.AddScoped<AiRichTextToolbarUpgrader>();

        builder.Services.AddScoped<ISeedStep, LanguagesStep>();
        builder.Services.AddScoped<ISeedStep, SchemaStep>();
        builder.Services.AddScoped<ISeedStep, DictionaryStep>();
        builder.Services.AddScoped<ISeedStep, StarterContentStep>();
        builder.Services.AddScoped<ISeedStep, AiSetupStep>();
        builder.Services.AddScoped<ISeedStep, AiSearchStep>();

        builder.AddNotificationAsyncHandler<UmbracoApplicationStartedNotification, SeedRunner>();
        // Runs on every start (not a seed step) so a rotated secret in .env always wins.
        builder.AddNotificationAsyncHandler<UmbracoApplicationStartedNotification, McpApiUserSetup>();
    }
}
