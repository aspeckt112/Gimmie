namespace Gimmie.Cli.IntegrationTests.Infrastructure;

/// <summary>The captured result of running the Gimmie CLI.</summary>
internal sealed record CliResult(int ExitCode, string StdOut, string StdErr)
{
    /// <summary>Stdout split into lines, with any trailing newline removed.</summary>
    public string[] StdOutLines =>
        StdOut.Length == 0
            ? []
            : StdOut.TrimEnd('\n').Split('\n');
}
