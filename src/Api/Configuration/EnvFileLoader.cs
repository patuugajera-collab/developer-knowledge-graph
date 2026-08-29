using System.Globalization;

namespace DeveloperKnowledgeGraph.Api.Configuration;

/// <summary>
/// Minimal .env loader used for local development convenience.
/// Environment variables set in the real environment always take precedence.
/// </summary>
public static class EnvFileLoader
{
    public static void Load(string path, bool optional = true)
    {
        if (!File.Exists(path))
        {
            if (!optional)
            {
                throw new FileNotFoundException($"Environment file not found at '{path}'.", path);
            }

            return;
        }

        foreach (var rawLine in File.ReadAllLines(path))
        {
            var line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal))
            {
                continue;
            }

            var separatorIndex = line.IndexOf('=');
            if (separatorIndex <= 0)
            {
                continue;
            }

            var key = line[..separatorIndex].Trim();
            var value = line[(separatorIndex + 1)..].Trim();

            value = Unquote(value);

            var existing = Environment.GetEnvironmentVariable(key);
            if (string.IsNullOrEmpty(existing))
            {
                Environment.SetEnvironmentVariable(key, value);
            }
        }
    }

    private static string Unquote(string value)
    {
        if (value.Length >= 2)
        {
            var first = value[0];
            var last = value[^1];
            if ((first == '"' && last == '"') || (first == '\'' && last == '\''))
            {
                return value[1..^1];
            }
        }

        return value;
    }

    public static void LogLoadedVariables(string envFilePath)
    {
        if (!File.Exists(envFilePath))
        {
            Console.WriteLine("No .env file found - relying on process environment variables.");
            return;
        }

        var keys = File.ReadAllLines(envFilePath)
            .Where(l => !string.IsNullOrWhiteSpace(l) && !l.Trim().StartsWith('#') && l.Contains('='))
            .Select(l => l.Split('=')[0].Trim())
            .Where(k => k.StartsWith("CORS_", StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (keys.Count > 0)
        {
            var detected = string.Join(", ", keys);
            Console.WriteLine($"Loaded configuration keys from {envFilePath}: {detected}");
        }
    }
}