namespace Gimmie.Cli.Commands;

/// <summary>Generates a specified number of GUIDs and writes them to the console.</summary>
public static class GuidsCommand
{
    /// <param name="number">-n, Number of GUIDs to generate.</param>
    /// <param name="upper">-u, Uppercase the generated GUIDs.</param>
    /// <param name="separator">-s, Separator between GUIDs (default: newline).</param>
    public static void Run(int number = 1, bool upper = false, char? separator = null)
    {
        if (number <= 0)
        {
            return;
        }

        char sep = separator ?? '\n';

        for (int i = 0; i < number; i++)
        {
            if (i > 0)
            {
                Console.Write(sep);
            }

            Console.Write(GuidFactory.New(upper));
        }

        Console.WriteLine();
    }
}