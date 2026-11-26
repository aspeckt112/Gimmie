using ConsoleAppFramework;

namespace Gimmie.Cli.Commands;

/// <summary>Shows available random generators.</summary>
internal static class RandomCommand
{
    /// <param name="subcommand">Random generator to use.</param>
    public static void Run([Argument] string? subcommand = null)
    {
        if (subcommand is not null)
        {
            Console.Error.WriteLine($"Unknown random subcommand: {subcommand}");
            Environment.ExitCode = 1;
            return;
        }

        ShowHelp();
    }

    internal static bool TryShowHelp(string[] arguments)
    {
        // TODO More ConsoleAppFramework idiomatic way to do this?
        if (arguments.Length < 2 || arguments[0] != "random")
        {
            return false;
        }

        bool help = false;

        foreach (string argument in arguments.AsSpan(1))
        {
            if (argument is "--help" or "-h")
            {
                help = true;
            }
            else if (argument != "--copy")
            {
                return false;
            }
        }

        if (help)
        {
            ShowHelp();
        }

        return help;
    }

    private static void ShowHelp() => Console.WriteLine("""
            Usage: random [command] [-h|--help] [--version]

            Commands:
              integer    Generate a random integer.
              choice     Select a random choice.

            Options:
              --copy     Copy a command's result to the clipboard.

            Run 'gimmie random <command> --help' for command options.
            """);
}