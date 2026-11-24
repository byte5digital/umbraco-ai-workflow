using UmbracoAI.Web.Search;

namespace UmbracoAI.Web.Tests;

public sealed class ContentSearchServiceTests
{
    [Theory]
    [InlineData("parken AND \"terminal\"*", "parken terminal")]
    [InlineData("  OR  ", "")]
    [InlineData("Zürich 2026", "Zürich 2026")]
    public void ToSearchTerms_StripsLuceneSyntax(string query, string expected) =>
        Assert.Equal(expected, ContentSearchService.ToSearchTerms(query));

    [Fact]
    public void Fuse_RanksDocumentsFoundByBothSearchesFirst()
    {
        Guid onlySemantic = Guid.NewGuid();
        Guid both = Guid.NewGuid();
        Guid onlyText = Guid.NewGuid();

        var result = ContentSearchService.Fuse([onlySemantic, both], [both, onlyText]);

        Assert.Equal(both, result[0].Key);
        Assert.True(result[0].IsSemanticMatch);
        Assert.True(result[0].IsFullTextMatch);
        Assert.Equal(3, result.Count);
    }
}
