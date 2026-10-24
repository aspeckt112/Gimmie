namespace Gimmie.Cli.IntegrationTests.Tests;

public sealed class CliSurfaceTests
{
    [Fact]
    public async Task Help_NoArguments_ListsCommandsAndExitsZero()
    {
        CliResult result = await CliRunner.RunAsync("--help");

        Equal(0, result.ExitCode);
        Contains("Usage:", result.StdOut);
        Contains("guid", result.StdOut);
        Contains("guids", result.StdOut);
    }

    [Fact]
    public async Task Help_GuidCommand_DescribesUpperOption()
    {
        CliResult result = await CliRunner.RunAsync("guid", "--help");

        Equal(0, result.ExitCode);
        Contains("-u", result.StdOut);
    }

    [Fact]
    public async Task Help_GuidsCommand_DescribesNumberAndSeparatorOptions()
    {
        CliResult result = await CliRunner.RunAsync("guids", "--help");

        Equal(0, result.ExitCode);
        Contains("--number", result.StdOut);
        Contains("--separator", result.StdOut);
    }

    [Fact]
    public async Task VersionFlag_Invoked_PrintsSemanticVersionAndExitsZero()
    {
        CliResult result = await CliRunner.RunAsync("--version");

        Equal(0, result.ExitCode);
        Matches(@"^\d+\.\d+\.\d+", result.StdOut.TrimEnd('\n'));
    }

    [Fact]
    public async Task UnrecognizedOption_Supplied_ExitsWithCodeOne()
    {
        CliResult result = await CliRunner.RunAsync("guids", "--bogus");

        Equal(1, result.ExitCode);
        NotEmpty(result.StdOut);
        Empty(result.StdErr);
    }

    [Fact]
    public async Task NonNumericNumber_Supplied_ExitsWithCodeOne()
    {
        CliResult result = await CliRunner.RunAsync("guids", "-n", "abc");

        Equal(1, result.ExitCode);
        NotEmpty(result.StdOut);
        Empty(result.StdErr);
    }

    // NOTE: ConsoleAppFramework currently prints usage to stdout and exits 0 for an
    // unrecognized command. That is a suspected bug; this test pins the current
    // behaviour so a fix is a deliberate, visible change.
    [Fact]
    public async Task UnrecognizedCommand_Supplied_CurrentlyWritesUsageAndExitsZero()
    {
        CliResult result = await CliRunner.RunAsync("nope");

        Equal(0, result.ExitCode);
        Contains("Usage:", result.StdOut);
    }

    // ConsoleAppFramework writes diagnostics to stdout, not stderr. Pinned so a
    // framework upgrade that starts using stderr is caught.
    [Fact]
    public async Task Diagnostics_InvalidOption_AreWrittenToStdoutNotStderr()
    {
        CliResult result = await CliRunner.RunAsync("guids", "--bogus");

        Empty(result.StdErr);
    }
}
