namespace UmbracoAI.Web.Seeding;

/// <summary>Outcome of a seed step.</summary>
public enum SeedStepResult
{
    /// <summary>Done – the version is recorded and the step does not run again.</summary>
    Completed,

    /// <summary>Preconditions missing (e.g. no API key) – nothing recorded, retried on the next start.</summary>
    Skipped,
}

/// <summary>
/// One idempotent, versioned seed step (migration-like). <see cref="SeedRunner"/> runs all steps ordered by
/// <see cref="Order"/> and stores <see cref="Version"/> per <see cref="Id"/> in Umbraco's key/value table.
/// Bump the version to re-run a step after changing it – steps must therefore be safe on existing installs.
/// </summary>
public interface ISeedStep
{
    /// <summary>Stable id, used as idempotency key ("schema", "starter-content" …).</summary>
    string Id { get; }

    int Version { get; }

    /// <summary>Execution order. Leave gaps (100, 200 …) so project steps can be slotted in between.</summary>
    int Order { get; }

    Task<SeedStepResult> ExecuteAsync(CancellationToken cancellationToken);
}
