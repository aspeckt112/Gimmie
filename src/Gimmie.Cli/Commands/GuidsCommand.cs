using System.Text;

using ConsoleAppFramework;

using Gimmie.Cli.Output;

namespace Gimmie.Cli.Commands;

/// <summary>Generates a specified number of GUIDs and writes them to the console.</summary>
internal static class GuidsCommand
{
    /// <param name="context">The shared command-response context.</param>
    /// <param name="number">-n, Number of GUIDs to generate.</param>
    /// <param name="upper">-u, Uppercase the generated GUIDs.</param>
    /// <param name="separator">-s, Separator between GUIDs (default: newline).</param>
    public static void Run(ConsoleAppContext context, int number = 1, bool upper = false, char? separator = null)
    {
        if (number <= 0)
        {
            return;
        }

        char sep = separator ?? '\n';
        var response = new StringBuilder();

        for (int i = 0; i < number; i++)
        {
            if (i > 0)
            {
                _ = response.Append(sep);
            }

            _ = response.Append(GuidFactory.New(upper));
        }

        context.WriteResponse(response.ToString());
    }
}