using Opten.Web.Site;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Opten.Web.Seeding.Steps;

/// <summary>Creates the UI dictionary (<see cref="UiTexts.All"/>). Existing items are left untouched (editors may have changed them).</summary>
public sealed class DictionaryStep(
    ILanguageService languageService,
    IDictionaryItemService dictionaryItemService) : ISeedStep
{
    public string Id => "dictionary";
    public int Version => 1;
    public int Order => 300;

    public async Task<SeedStepResult> ExecuteAsync(CancellationToken cancellationToken)
    {
        var languages = (await languageService.GetAllAsync()).ToDictionary(l => l.IsoCode, StringComparer.OrdinalIgnoreCase);

        foreach (UiText text in UiTexts.All)
        {
            if (await dictionaryItemService.GetAsync(text.Key) is not null)
            {
                continue;
            }

            var item = new DictionaryItem(text.Key)
            {
                Translations =
                [
                    new DictionaryTranslation(languages[Cultures.German], text.German),
                    new DictionaryTranslation(languages[Cultures.English], text.English),
                ],
            };

            var result = await dictionaryItemService.CreateAsync(item, Constants.Security.SuperUserKey);
            SeedGuard.Ensure(result.Success, $"creating dictionary item {text.Key}", result.Status);
        }

        return SeedStepResult.Completed;
    }
}
