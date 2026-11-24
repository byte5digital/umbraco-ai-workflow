using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace UmbracoAI.Web.Seeding.Mcp;

/// <summary>
/// Ensures the API user (administrators group) and client credentials for the local Umbraco Developer MCP server.
/// Runs on every start so the secret from configuration always wins (rotation = change env + restart).
/// Development environment only – the default secret is published in .env.example.
/// </summary>
public sealed class McpApiUserSetup(
    IRuntimeState runtimeState,
    IHostEnvironment hostEnvironment,
    IServiceProvider serviceProvider,
    IOptions<McpApiUserOptions> options,
    ILogger<McpApiUserSetup> logger)
    : INotificationAsyncHandler<UmbracoApplicationStartedNotification>
{
    private static readonly Guid PerformingUserKey = Constants.Security.SuperUserKey;

    public async Task HandleAsync(UmbracoApplicationStartedNotification notification, CancellationToken cancellationToken)
    {
        McpApiUserOptions settings = options.Value;
        if (runtimeState.Level != RuntimeLevel.Run || string.IsNullOrWhiteSpace(settings.ClientSecret))
        {
            return;
        }

        // The secret in .env.example is public – the admin API user must never exist outside local development.
        if (!hostEnvironment.IsDevelopment())
        {
            logger.LogWarning("MCP setup skipped: the MCP API user is only created in the Development environment");
            return;
        }

        if (!settings.ClientId.StartsWith(McpApiUserOptions.ClientIdPrefix, StringComparison.Ordinal))
        {
            logger.LogError("MCP setup: client id must start with {Prefix} – skipped", McpApiUserOptions.ClientIdPrefix);
            return;
        }

        try
        {
            using IServiceScope scope = serviceProvider.CreateScope();
            Guid userKey = await EnsureApiUserAsync(scope.ServiceProvider.GetRequiredService<IUserService>(), settings);
            await EnsureClientCredentialsAsync(
                scope.ServiceProvider.GetRequiredService<IBackOfficeUserClientCredentialsManager>(), userKey, settings);
            logger.LogInformation("MCP setup: API user {Email} ready with client id {ClientId}", settings.Email, settings.ClientId);
        }
        catch (Exception ex)
        {
            // The site works without MCP; retried on the next start.
            logger.LogError(ex, "MCP setup failed – will retry on next start");
        }
    }

    private async Task<Guid> EnsureApiUserAsync(IUserService userService, McpApiUserOptions settings)
    {
        IUser? existing = userService.GetByEmail(settings.Email);
        if (existing is not null)
        {
            return existing.Key;
        }

        var model = new UserCreateModel
        {
            Email = settings.Email,
            UserName = settings.Email,
            Name = settings.Name,
            Kind = UserKind.Api,
            UserGroupKeys = new HashSet<Guid> { Constants.Security.AdminGroupKey },
        };

        var created = await userService.CreateAsync(PerformingUserKey, model, approveUser: true);
        Guid userKey = created.Result.CreatedUser?.Key
            ?? throw new InvalidOperationException($"Could not create API user '{settings.Email}': {created.Status}");

        logger.LogInformation("MCP setup: created API user {Email}", settings.Email);
        return userKey;
    }

    private static async Task EnsureClientCredentialsAsync(
        IBackOfficeUserClientCredentialsManager manager, Guid userKey, McpApiUserOptions settings)
    {
        IEnumerable<string> clientIds = await manager.GetClientIdsAsync(userKey);
        if (clientIds.Contains(settings.ClientId))
        {
            var deleted = await manager.DeleteAsync(userKey, settings.ClientId);
            if (!deleted.Success)
            {
                throw new InvalidOperationException($"Could not replace client credentials: {deleted.Result}");
            }
        }

        var saved = await manager.SaveAsync(userKey, settings.ClientId, settings.ClientSecret!);
        if (!saved.Success)
        {
            throw new InvalidOperationException($"Could not save client credentials '{settings.ClientId}': {saved.Result}");
        }
    }
}
