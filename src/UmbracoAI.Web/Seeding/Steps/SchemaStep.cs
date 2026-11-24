using UmbracoAI.Web.Seeding.Schema;

namespace UmbracoAI.Web.Seeding.Steps;

/// <summary>
/// Creates data types, element types, compositions, templates and page types. Idempotent: existing types are skipped,
/// compositions are brought up to date. Version history: 1 = initial model, 2 = GEO preview ("llmsPreview").
/// </summary>
public sealed class SchemaStep(SchemaSeeder schemaSeeder) : ISeedStep
{
    public string Id => "schema";
    public int Version => 2;
    public int Order => 200;

    public async Task<SeedStepResult> ExecuteAsync(CancellationToken cancellationToken)
    {
        await schemaSeeder.CreateAsync();
        return SeedStepResult.Completed;
    }
}
