using System.Text.RegularExpressions;

namespace Mvp.Cli.Features.Generation;

public static partial class Naming
{
    public static string ToKebabCase(string value)
    {
        var normalized = AcronymBoundary().Replace(value, "$1-$2");
        normalized = WordBoundary().Replace(normalized, "$1-$2");
        normalized = NonAlphaNumeric().Replace(normalized, "-");
        return normalized.Trim('-').ToLowerInvariant();
    }

    public static string ToCamelCase(string value) =>
        value.Length == 0 ? value : char.ToLowerInvariant(value[0]) + value[1..];

    [GeneratedRegex("([A-Z]+)([A-Z][a-z])", RegexOptions.CultureInvariant)]
    private static partial Regex AcronymBoundary();

    [GeneratedRegex("([a-z0-9])([A-Z])", RegexOptions.CultureInvariant)]
    private static partial Regex WordBoundary();

    [GeneratedRegex("[^A-Za-z0-9]+", RegexOptions.CultureInvariant)]
    private static partial Regex NonAlphaNumeric();
}
