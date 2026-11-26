using ConsoleAppFramework;

using Gimmie.Cli.Output;

namespace Gimmie.Cli.Commands;

/// <summary>Selects one cryptographically secure random choice.</summary>
internal static class RandomChoiceCommand
{
    /// <param name="context">The shared command-response context.</param>
    /// <param name="choices">Separate arguments take precedence over redirected stdin lines. Empty entries are ignored; duplicates remain separate. Use -- to escape options.</param>
    public static async Task Run(ConsoleAppContext context, [Argument] params string[] choices)
    {
        string[] escapedChoices = context.EscapedArguments.ToArray();
        var entries = new List<string>();

        if (choices.Length > 0 || escapedChoices.Length > 0)
        {
            entries.AddRange(choices.Where(choice => choice.Length > 0));
            entries.AddRange(escapedChoices.Where(choice => choice.Length > 0));
        }
        else if (Console.IsInputRedirected)
        {
            while (await Console.In.ReadLineAsync() is { } line)
            {
                if (line.Length > 0)
                {
                    entries.Add(line);
                }
            }
        }

        if (entries.Count == 0)
        {
            await Console.Error.WriteLineAsync("At least one nonempty choice is required.");
            Environment.ExitCode = 1;
            return;
        }

        context.WriteResponse(RandomSelection.Choice(entries));
    }
}