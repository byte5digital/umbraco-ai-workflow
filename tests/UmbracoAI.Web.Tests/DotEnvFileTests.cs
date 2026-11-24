using UmbracoAI.Web.Configuration;

namespace UmbracoAI.Web.Tests;

public sealed class DotEnvFileTests
{
    [Fact]
    public void Parse_ReadsKeyValuePairs_AndSkipsCommentsAndEmptyValues()
    {
        string[] lines =
        [
            "# comment",
            "",
            "Site__Name=Umbraco AI Starterkit",
            "export Seed__Enabled=true",
            "Quoted=\"with spaces\"",
            "Empty=",
            "invalid line",
        ];

        var result = DotEnvFile.Parse(lines);

        Assert.Equal(
            [("Site__Name", "Umbraco AI Starterkit"), ("Seed__Enabled", "true"), ("Quoted", "with spaces")],
            result);
    }

    [Fact]
    public void Load_DoesNotOverrideExistingEnvironmentVariables()
    {
        string key = $"STARTERKIT_TEST_{Guid.NewGuid():N}";
        string path = Path.GetTempFileName();
        try
        {
            File.WriteAllText(path, $"{key}=from-file\n");
            Environment.SetEnvironmentVariable(key, "from-env");

            int applied = DotEnvFile.Load(path);

            Assert.Equal(0, applied);
            Assert.Equal("from-env", Environment.GetEnvironmentVariable(key));
        }
        finally
        {
            Environment.SetEnvironmentVariable(key, null);
            File.Delete(path);
        }
    }
}
