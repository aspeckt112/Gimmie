using CliWrap;
using CliWrap.Buffered;

// This project's namespace nests under 'Gimmie.Cli', which shadows CliWrap's 'Cli'
// factory, so alias it to keep 'Wrap' unambiguous.
using CliFactory = CliWrap.Cli;

namespace Gimmie.Cli.IntegrationTests.Infrastructure;

/// <summary>Runs the real Gimmie.Cli executable and captures its output.</summary>
internal static class CliRunner
{
    private static readonly string CliDll = CliDllPath.Resolve();

    public static async Task<CliResult> RunAsync(params string[] args)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));

        BufferedCommandResult result = await CliFactory.Wrap("dotnet")
            .WithArguments([CliDll, .. args])
            .WithValidation(CommandResultValidation.None)
            .ExecuteBufferedAsync(timeout.Token);

        return new CliResult(
            result.ExitCode,
            Normalize(result.StandardOutput),
            Normalize(result.StandardError));
    }

    // The CLI writes via Console.WriteLine, which becomes \r\n on Windows.
    // Normalizing keeps assertions identical across operating systems.
    private static string Normalize(string value) => value.Replace("\r\n", "\n");
}
