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
        var options = (ResponseOptions)context.GlobalOptions!;
        StringBuilder? response = options.Copy ? new StringBuilder() : null;

        for (int i = 0; i < number; i++)
        {
            if (i > 0)
            {
                if (response is null)
                {
                    Console.Out.Write(sep);
                }
                else
                {
                    _ = response.Append(sep);
                }
            }

            string guid = GuidFactory.New(upper);
            if (response is null)
            {
                Console.Out.Write(guid);
            }
            else
            {
                _ = response.Append(guid);
            }
        }

        if (response is null)
        {
            Console.Out.WriteLine();
        }
        else
        {
            context.WriteResponse(response.ToString());
        }
    }
}