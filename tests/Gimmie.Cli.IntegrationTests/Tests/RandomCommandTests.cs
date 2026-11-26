namespace Gimmie.Cli.IntegrationTests.Tests;

public sealed class RandomCommandTests
{
    [Theory]
    [InlineData("random")]
    [InlineData("random", "--help")]
    [InlineData("random", "-h")]
    [InlineData("--copy", "random", "--help")]
    [InlineData("random", "--copy", "--help")]
    [InlineData("random", "--help", "--copy")]
    [InlineData("random", "--copy")]
    public async Task Random_GroupHelp_ListsOnlyRandomSubcommands(params string[] args)
    {
        CliResult result = await CliRunner.RunAsync(args);

        Equal(0, result.ExitCode);
        Empty(result.StdErr);
        Contains("Usage: random [command]", result.StdOut);
        Contains("integer", result.StdOut);
        Contains("choice", result.StdOut);
        Contains("--copy", result.StdOut);
        DoesNotContain("password", result.StdOut);
        DoesNotContain("guid", result.StdOut);
    }

    [Fact]
    public async Task Random_UnknownSubcommand_Fails()
    {
        CliResult result = await CliRunner.RunAsync("random", "unknown");

        NotEqual(0, result.ExitCode);
        Contains("unknown", result.StdErr);
    }

    [Theory]
    [InlineData(0, 100)]
    [InlineData(-10, -1, "--min", "-10", "--max", "-1")]
    [InlineData(-4, 4, "--min", "-4", "--max", "4")]
    [InlineData(-7, -7, "--min", "-7", "--max", "-7")]
    [InlineData(int.MinValue, int.MaxValue, "--min", "-2147483648", "--max", "2147483647")]
    [InlineData(int.MaxValue, int.MaxValue, "--min", "2147483647", "--max", "2147483647")]
    [InlineData(int.MinValue, int.MinValue, "--min", "-2147483648", "--max", "-2147483648")]
    public async Task Integer_ProducesOneValueWithinInclusiveBounds(int min, int max, params string[] args)
    {
        CliResult result = await CliRunner.RunAsync(["random", "integer", .. args]);

        Equal(0, result.ExitCode);
        Empty(result.StdErr);
        True(result.StdOut.EndsWith('\n'));
        string value = Single(result.StdOutLines);
        True(int.TryParse(value, out int number));
        InRange(number, min, max);
    }

    [Theory]
    [InlineData("--min", "abc")]
    [InlineData("--max", "1.5")]
    [InlineData("--min", "-2147483649")]
    [InlineData("--max", "2147483648")]
    [InlineData("--min")]
    [InlineData("--min", "1", "--max", "0")]
    public async Task Integer_InvalidBounds_FailWithDiagnostic(params string[] args)
    {
        CliResult result = await CliRunner.RunAsync(["random", "integer", "--copy", .. args]);

        NotEqual(0, result.ExitCode);
        NotEmpty(result.StdOut + result.StdErr);
        DoesNotContain("Clipboard copy failed", result.StdOut + result.StdErr);
    }

    [Theory]
    [InlineData("  keep spaces  ")]
    [InlineData("   ")]
    [InlineData("λ")]
    public async Task Choice_SinglePositionalChoice_PreservesText(string choice)
    {
        CliResult result = await CliRunner.RunAsync("random", "choice", "", choice, "");

        Equal(0, result.ExitCode);
        Equal(choice + "\n", result.StdOut);
        Empty(result.StdErr);
    }

    [Fact]
    public async Task Choice_MultipleArguments_ReturnsOneEntry()
    {
        CliResult result = await CliRunner.RunAsync("random", "choice", "one", "two", "two");

        Equal(0, result.ExitCode);
        Contains(Single(result.StdOutLines), new[] { "one", "two" });
        Empty(result.StdErr);
    }

    [Fact]
    public async Task Choice_Stdin_IgnoresEmptyLinesAndPreservesWhitespace()
    {
        CliResult result = await CliRunner.RunWithInputAsync("\r\n  value  \r\n\n", "random", "choice");

        Equal(0, result.ExitCode);
        Equal("  value  \n", result.StdOut);
        Empty(result.StdErr);
    }

    [Fact]
    public async Task Choice_ArgumentsTakePrecedenceOverStdin()
    {
        CliResult result = await CliRunner.RunWithInputAsync("stdin\n", "random", "choice", "argument");

        Equal(0, result.ExitCode);
        Equal("argument\n", result.StdOut);
    }

    [Fact]
    public async Task Choice_EmptyArgumentsStillTakePrecedenceOverStdin()
    {
        CliResult result = await CliRunner.RunWithInputAsync("stdin\n", "random", "choice", "");

        NotEqual(0, result.ExitCode);
        Empty(result.StdOut);
        Contains("choice", result.StdErr);
    }

    [Theory]
    [InlineData("")]
    [InlineData("\n\r\n\n")]
    public async Task Choice_EmptyStdin_Fails(string input)
    {
        CliResult result = await CliRunner.RunWithInputAsync(input, "random", "choice", "--copy");

        NotEqual(0, result.ExitCode);
        Empty(result.StdOut);
        Contains("choice", result.StdErr);
        DoesNotContain("Clipboard copy failed", result.StdErr);
    }

    [Theory]
    [InlineData("--copy")]
    [InlineData("--help")]
    [InlineData("-5")]
    public async Task Choice_EscapedArguments_AreChoices(string choice)
    {
        CliResult result = await CliRunner.RunWithInputAsync("stdin\n", "random", "choice", "--", choice);

        Equal(0, result.ExitCode);
        Equal(choice + "\n", result.StdOut);
        Empty(result.StdErr);
    }

    [Fact]
    public async Task Choice_EmptyEscapedArgumentStillTakesPrecedence()
    {
        CliResult result = await CliRunner.RunWithInputAsync("stdin\n", "random", "choice", "--", "");

        NotEqual(0, result.ExitCode);
        Empty(result.StdOut);
        Contains("choice", result.StdErr);
    }

    [Fact]
    public async Task Choice_OrdinaryAndEscapedArguments_AreBothChoices()
    {
        CliResult result = await CliRunner.RunAsync("random", "choice", "", "--", "escaped");

        Equal(0, result.ExitCode);
        Equal("escaped\n", result.StdOut);
    }

    [Theory]
    [InlineData("integer")]
    [InlineData("choice")]
    public async Task Random_Help_DescribesCommandAndCopy(string command)
    {
        CliResult result = await CliRunner.RunAsync("random", command, "--help");

        Equal(0, result.ExitCode);
        Contains("--copy", result.StdOut);
        if (command == "integer")
        {
            Contains("--min", result.StdOut);
            Contains("--max", result.StdOut);
            Contains("0", result.StdOut);
            Contains("100", result.StdOut);
            Contains("32-bit", result.StdOut);
        }
        else
        {
            Contains("stdin", result.StdOut);
            Contains("precedence", result.StdOut);
            Contains("duplicates", result.StdOut);
        }
    }
}