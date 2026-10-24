namespace Gimmie.Cli.IntegrationTests.Tests;

public sealed class GuidCommandTests
{
    [Fact]
    public async Task Guid_NoArguments_WritesOneLowercaseGuidAndExitsZero()
    {
        CliResult result = await CliRunner.RunAsync("guid");

        Equal(0, result.ExitCode);
        Empty(result.StdErr);
        Matches(GuidPatterns.Single(), Single(result.StdOutLines));
        EndsWith("\n", result.StdOut);
    }

    [Theory]
    [InlineData("-u")]
    [InlineData("--upper")]
    public async Task Guid_UpperFlag_WritesUppercaseGuid(string flag)
    {
        CliResult result = await CliRunner.RunAsync("guid", flag);

        Equal(0, result.ExitCode);
        Matches(GuidPatterns.Single(upper: true), Single(result.StdOutLines));
    }

    [Fact]
    public async Task Guid_ConsecutiveRuns_ProduceDifferentGuids()
    {
        CliResult first = await CliRunner.RunAsync("guid");
        CliResult second = await CliRunner.RunAsync("guid");

        NotEqual(first.StdOut, second.StdOut);
    }
}
