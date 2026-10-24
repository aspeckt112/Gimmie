using System.Text.RegularExpressions;

namespace Gimmie.Cli.IntegrationTests.Infrastructure;

/// <summary>Regex patterns for asserting on randomly generated GUIDs.</summary>
internal static class GuidPatterns
{
    private const string LowercaseGuid =
        "[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}";

    private const string UppercaseGuid =
        "[0-9A-F]{8}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{12}";

    /// <summary>A single GUID occupying an entire line.</summary>
    public static string Single(bool upper = false) => $"^{Body(upper)}$";

    /// <summary>
    /// <paramref name="count"/> GUIDs joined by <paramref name="separator"/>,
    /// followed by the newline the CLI always writes.
    /// </summary>
    public static string Sequence(int count, string separator = "\n", bool upper = false)
    {
        string guid = Body(upper);
        return $"^{guid}({Regex.Escape(separator)}{guid}){{{count - 1}}}\\n$";
    }

    private static string Body(bool upper) => upper ? UppercaseGuid : LowercaseGuid;
}
