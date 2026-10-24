namespace Gimmie.Cli.IntegrationTests.Tests;

public sealed class GuidsCommandTests
{
    [Fact]
    public async Task Guids_NoArguments_WritesSingleGuid()
    {
        CliResult result = await CliRunner.RunAsync("guids");

        Equal(0, result.ExitCode);
        Matches(GuidPatterns.Single(), Single(result.StdOutLines));
    }

    [Fact]
    public async Task Guids_NumberFlag_WritesRequestedNewlineSeparatedGuids()
    {
        CliResult result = await CliRunner.RunAsync("guids", "-n", "5");

        Equal(0, result.ExitCode);
        string[] lines = result.StdOutLines;
        Equal(5, lines.Length);
        All(lines, line => Matches(GuidPatterns.Single(), line));
        EndsWith("\n", result.StdOut);
    }

    [Fact]
    public async Task Guids_LongFormNumberFlag_IsAccepted()
    {
        CliResult result = await CliRunner.RunAsync("guids", "--number", "3");

        Equal(0, result.ExitCode);
        Equal(3, result.StdOutLines.Length);
    }

    [Fact]
    public async Task Guids_SeparatorFlag_JoinsGuidsOnOneLine()
    {
        CliResult result = await CliRunner.RunAsync("guids", "-n", "3", "-s", ",");

        Equal(0, result.ExitCode);
        Matches(GuidPatterns.Sequence(3, ","), result.StdOut);
    }

    [Fact]
    public async Task Guids_UpperAndSeparatorFlags_Combine()
    {
        CliResult result = await CliRunner.RunAsync("guids", "-n", "2", "-u", "-s", ";");

        Equal(0, result.ExitCode);
        Matches(GuidPatterns.Sequence(2, ";", upper: true), result.StdOut);
    }

    [Fact]
    public async Task Guids_MultipleGenerated_AreUnique()
    {
        CliResult result = await CliRunner.RunAsync("guids", "-n", "50");

        string[] lines = result.StdOutLines;
        Equal(50, lines.Length);
        Equal(50, lines.Distinct().Count());
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    public async Task Guids_NonPositiveCount_WritesNothingAndExitsZero(string number)
    {
        CliResult result = await CliRunner.RunAsync("guids", "-n", number);

        Equal(0, result.ExitCode);
        Empty(result.StdOut);
        Empty(result.StdErr);
    }
}
