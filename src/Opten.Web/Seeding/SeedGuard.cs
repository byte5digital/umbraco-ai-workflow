namespace Opten.Web.Seeding;

internal static class SeedGuard
{
    /// <summary>Umbraco services return attempts instead of throwing – turn failures into exceptions with context.</summary>
    public static void Ensure(bool success, string operation, object? status)
    {
        if (!success)
        {
            throw new InvalidOperationException($"Seed: {operation} failed: {status}");
        }
    }
}
