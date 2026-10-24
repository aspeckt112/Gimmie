namespace Gimmie.Cli.Commands;

/// <summary>Generates new GUIDs in the requested case.</summary>
internal static class GuidFactory
{
    /// <param name="upper">Uppercase the generated GUID.</param>
    public static string New(bool upper = false)
    {
        string guid = Guid.NewGuid().ToString();
        return upper ? guid.ToUpperInvariant() : guid;
    }
}