using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace UmbracoAI.Web.Seo;

/// <summary>Flattens a Block Grid (incl. areas and nested Block Lists) into its elements in document order.</summary>
public static class BlockTraversal
{
    public static IEnumerable<IPublishedElement> AllElements(this BlockGridModel? grid) =>
        grid is null ? [] : grid.SelectMany(GridItemElements);

    private static IEnumerable<IPublishedElement> GridItemElements(BlockGridItem item) =>
        ElementWithNested(item.Content).Concat(item.Areas.SelectMany(area => area).SelectMany(GridItemElements));

    private static IEnumerable<IPublishedElement> ElementWithNested(IPublishedElement element)
    {
        yield return element;
        foreach (IPublishedProperty property in element.Properties)
        {
            if (property.GetValue() is BlockListModel list)
            {
                foreach (IPublishedElement nested in list.SelectMany(item => ElementWithNested(item.Content)))
                {
                    yield return nested;
                }
            }
        }
    }
}
