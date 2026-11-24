using OpenIddict.Server.AspNetCore;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace UmbracoAI.Web.Configuration;

/// <summary>
/// Allows the backoffice login (OpenIddict) over plain HTTP – for Docker on localhost only.
/// Active only when <c>LocalDev:AllowHttpBackoffice</c> is explicitly true and the environment is Development.
/// </summary>
public sealed class LocalHttpComposer : IComposer
{
    public const string AllowHttpKey = "LocalDev:AllowHttpBackoffice";

    public void Compose(IUmbracoBuilder builder)
    {
        if (!builder.Config.GetValue<bool>(AllowHttpKey) || !string.Equals(
                builder.Config["ASPNETCORE_ENVIRONMENT"] ?? builder.Config["DOTNET_ENVIRONMENT"], "Development", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        builder.Services.Configure<OpenIddictServerAspNetCoreOptions>(options => options.DisableTransportSecurityRequirement = true);
    }
}
