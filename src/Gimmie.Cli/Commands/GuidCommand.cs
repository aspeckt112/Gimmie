using ConsoleAppFramework;

using Gimmie.Cli.Output;

namespace Gimmie.Cli.Commands;

/// <summary>Generates a single GUID and writes it to the console.</summary>
internal static class GuidCommand
{
    /// <param name="context">The shared command-response context.</param>
    /// <param name="upper">-u, Uppercase the generated GUID.</param>
    public static void Run(ConsoleAppContext context, bool upper = false) =>
        context.WriteResponse(GuidFactory.New(upper));
}