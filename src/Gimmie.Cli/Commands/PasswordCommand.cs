using ConsoleAppFramework;

using Gimmie.Cli.Output;

namespace Gimmie.Cli.Commands;

/// <summary>Generates one secure password containing every enabled character class.</summary>
internal static class PasswordCommand
{
    /// <param name="context">The shared command-response context.</param>
    /// <param name="length">-l, Password length (default: 20; maximum: 1024; minimum: enabled-class count).</param>
    /// <param name="noNumbers">-n, Exclude numbers (lowercase and uppercase always enabled).</param>
    /// <param name="noSymbols">-s, Exclude symbols (ambiguous characters are always omitted).</param>
    public static void Run(ConsoleAppContext context, int length = PasswordFactory.DefaultLength,
        bool noNumbers = false, bool noSymbols = false)
    {
        if (PasswordFactory.GetLengthError(length, noNumbers, noSymbols) is { } error)
        {
            Console.Error.WriteLine(error);
            Environment.ExitCode = 1;
            return;
        }

        context.WriteResponse(PasswordFactory.New(length, noNumbers, noSymbols), sensitive: true);
    }
}