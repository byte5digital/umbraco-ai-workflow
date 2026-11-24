using UmbracoAI.Web.Seo;

namespace UmbracoAI.Web.Tests;

public sealed class CrawlerFilesTests
{
    [Fact]
    public void Robots_BlocksTrainingCrawlers_ButKeepsAiSearchAllowed()
    {
        string robots = CrawlerFiles.Robots("Test", blockAiTraining: true, "https://example.com/sitemap.xml", "https://example.com/de/llms.txt");

        Assert.Contains("User-agent: GPTBot", robots);
        Assert.Contains("User-agent: OAI-SearchBot\nUser-agent: ChatGPT-User", robots);
        Assert.Contains("Disallow: /\n", robots);
        Assert.Contains("Sitemap: https://example.com/sitemap.xml", robots);
    }

    [Theory]
    [InlineData(1, "1.0")]
    [InlineData(2, "0.8")]
    [InlineData(5, "0.6")]
    public void DefaultPriority_DependsOnLevel(int level, string expected) =>
        Assert.Equal(expected, CrawlerFiles.DefaultPriority(level));
}
