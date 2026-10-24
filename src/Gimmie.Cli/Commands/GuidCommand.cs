namespace Gimmie.Cli.Commands;

/// <summary>Generates a single GUID and writes it to the console.</summary>
public static class GuidCommand
{
    /// <param name="upper">-u, Uppercase the generated GUID.</param>
    public static void Run(bool upper = false) => Console.WriteLine(GuidFactory.New(upper));
}