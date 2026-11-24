using Umbraco.Cms.Core.Models;

namespace Opten.Web.Seeding.Schema;

/// <summary>
/// Brings a content type into the declared tab/group layout. Idempotent: creates missing tabs/groups, moves existing
/// properties (values are kept) and adds missing ones – usable for fresh installs and schema upgrades alike.
/// </summary>
public static class TabLayout
{
    public static void Apply(IContentTypeBase contentType, IEnumerable<GroupDef> groups, ContentTypeFactory factory)
    {
        foreach (GroupDef group in groups)
        {
            EnsureGroup(contentType, group.Tab.Alias, group.Tab.Name, PropertyGroupType.Tab, group.Tab.SortOrder);
            EnsureGroup(contentType, group.Alias, group.Name, PropertyGroupType.Group, group.SortOrder);

            int sortOrder = 0;
            foreach (PropertyDef property in group.Properties)
            {
                IPropertyType? existing = contentType.PropertyTypes.FirstOrDefault(p => p.Alias == property.Alias);
                if (existing is null)
                {
                    existing = factory.CreatePropertyType(property);
                    contentType.AddPropertyType(existing, group.Alias, group.Name);
                }
                else
                {
                    contentType.MovePropertyType(property.Alias, group.Alias);
                }

                existing.SortOrder = sortOrder++;
            }
        }
    }

    private static void EnsureGroup(IContentTypeBase contentType, string alias, string name, PropertyGroupType type, int sortOrder)
    {
        PropertyGroup? existing = contentType.PropertyGroups.FirstOrDefault(g => g.Alias == alias);
        if (existing is null)
        {
            contentType.PropertyGroups.Add(new PropertyGroup(isPublishing: contentType is IContentType)
            {
                Alias = alias,
                Name = name,
                Type = type,
                SortOrder = sortOrder,
            });
            return;
        }

        existing.Type = type;
        existing.Name = name;
        existing.SortOrder = sortOrder;
    }
}
