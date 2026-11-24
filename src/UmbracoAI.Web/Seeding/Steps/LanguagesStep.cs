using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using UmbracoAI.Web.Site;

namespace UmbracoAI.Web.Seeding.Steps;

/// <summary>Creates the site languages (<see cref="Cultures.All"/>, first = default).</summary>
public sealed class LanguagesStep(ILanguageService languageService) : ISeedStep
{
    public string Id => "languages";
    public int Version => 1;
    public int Order => 100;

    public async Task<SeedStepResult> ExecuteAsync(CancellationToken cancellationToken)
    {
        foreach (SiteCulture culture in Cultures.All)
        {
            bool isDefault = culture == Cultures.Default;
            ILanguage? language = await languageService.GetAsync(culture.IsoCode);
            if (language is null)
            {
                var created = await languageService.CreateAsync(
                    new Language(culture.IsoCode, culture.Name) { IsDefault = isDefault }, Constants.Security.SuperUserKey);
                SeedGuard.Ensure(created.Success, $"creating language {culture.IsoCode}", created.Status);
            }
            else if (isDefault && !language.IsDefault)
            {
                language.IsDefault = true;
                var updated = await languageService.UpdateAsync(language, Constants.Security.SuperUserKey);
                SeedGuard.Ensure(updated.Success, $"setting {culture.IsoCode} as default", updated.Status);
            }
        }

        return SeedStepResult.Completed;
    }
}
