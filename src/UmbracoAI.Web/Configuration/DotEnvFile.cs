namespace UmbracoAI.Web.Configuration;

/// <summary>
/// Loads a <c>.env</c> file into environment variables before the host is built, so local development works without
/// Docker or exported shell variables. Keys use the ASP.NET Core convention (<c>Section__Key</c>).
/// Variables that are already set (CI, container, shell) always win over the file.
/// </summary>
public static class DotEnvFile
{
    public const string FileName = ".env";

    /// <summary>Searches the content root and its parents (repository root) for the first <c>.env</c> file.</summary>
    public static string? Find(string startDirectory, int maxLevels = 3)
    {
        DirectoryInfo? directory = new(startDirectory);
        for (int level = 0; directory is not null && level <= maxLevels; level++, directory = directory.Parent)
        {
            string candidate = Path.Combine(directory.FullName, FileName);
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        return null;
    }

    /// <returns>Number of variables applied from the file.</returns>
    public static int Load(string? path)
    {
        if (path is null || !File.Exists(path))
        {
            return 0;
        }

        int applied = 0;
        foreach ((string key, string value) in Parse(File.ReadLines(path)))
        {
            if (Environment.GetEnvironmentVariable(key) is null)
            {
                Environment.SetEnvironmentVariable(key, value);
                applied++;
            }
        }

        return applied;
    }

    /// <summary>Parses KEY=VALUE lines. Supports comments (#), blank lines, "export " prefixes and quoted values.</summary>
    public static IReadOnlyList<(string Key, string Value)> Parse(IEnumerable<string> lines)
    {
        var result = new List<(string, string)>();
        foreach (string rawLine in lines)
        {
            string line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith('#'))
            {
                continue;
            }

            if (line.StartsWith("export ", StringComparison.Ordinal))
            {
                line = line["export ".Length..].TrimStart();
            }

            int separator = line.IndexOf('=');
            if (separator <= 0)
            {
                continue;
            }

            string key = line[..separator].Trim();
            string value = Unquote(line[(separator + 1)..].Trim());
            if (key.Length > 0 && value.Length > 0)
            {
                result.Add((key, value));
            }
        }

        return result;
    }

    private static string Unquote(string value) =>
        value.Length >= 2 && (value[0] == '"' || value[0] == '\'') && value[^1] == value[0]
            ? value[1..^1]
            : value;
}
