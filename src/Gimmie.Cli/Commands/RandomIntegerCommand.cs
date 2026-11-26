using System.Globalization;

using ConsoleAppFramework;

using Gimmie.Cli.Output;

namespace Gimmie.Cli.Commands;

/// <summary>Generates one cryptographically secure random integer.</summary>
internal static class RandomIntegerCommand
{
    /// <param name="context">The shared command-response context.</param>
    /// <param name="min">Inclusive signed 32-bit minimum (default: 0; must not exceed maximum).</param>
    /// <param name="max">Inclusive signed 32-bit maximum (default: 100).</param>
    public static void Run(ConsoleAppContext context, int min = 0, int max = 100)
    {
        if (min > max)
        {
            Console.Error.WriteLine("Minimum must not exceed maximum.");
            Environment.ExitCode = 1;
            return;
        }

        context.WriteResponse(RandomSelection.Integer(min, max).ToString(CultureInfo.InvariantCulture));
    }
}