using Gimmie.Cli.Commands;

namespace Gimmie.Cli.IntegrationTests.Tests;

public sealed class PasswordCommandTests
{
    private const string Lowercase = "abcdefghjkmnpqrstuvwxyz";
    private const string Uppercase = "ABCDEFGHJKMNPQRSTUVWXYZ";
    private const string Numbers = "23456789";
    private const string Symbols = "!#$%&()*+,-./:;<=>?@[]^_{|}~";

    [Fact]
    public async Task Password_Default_PrintsOneTwentyCharacterPassword()
    {
        CliResult result = await CliRunner.RunAsync("password");

        Equal(0, result.ExitCode);
        Empty(result.StdErr);
        AssertOutput(result.StdOut, 20, false, false);
    }

    [Theory]
    [InlineData(32, false, false, "--length", "32")]
    [InlineData(32, false, false, "-l", "32")]
    [InlineData(20, true, false, "--no-numbers")]
    [InlineData(20, true, false, "-n")]
    [InlineData(20, false, true, "--no-symbols")]
    [InlineData(20, false, true, "-s")]
    [InlineData(20, true, true, "--no-numbers", "--no-symbols")]
    [InlineData(20, true, true, "-n", "-s")]
    [InlineData(4, false, false, "-l", "4")]
    [InlineData(3, true, false, "-l", "3", "-n")]
    [InlineData(3, false, true, "-l", "3", "-s")]
    [InlineData(2, true, true, "-l", "2", "-n", "-s")]
    [InlineData(1024, false, false, "--length", "1024")]
    [InlineData(1024, true, false, "-l", "1024", "-n")]
    [InlineData(1024, false, true, "-l", "1024", "-s")]
    [InlineData(1024, true, true, "-l", "1024", "-n", "-s")]
    public async Task Password_Options_RespectLengthAndEnabledClasses(int length, bool noNumbers,
        bool noSymbols, params string[] args)
    {
        CliResult result = await CliRunner.RunAsync(["password", .. args]);

        Equal(0, result.ExitCode);
        Empty(result.StdErr);
        AssertOutput(result.StdOut, length, noNumbers, noSymbols);
    }

    [Theory]
    [InlineData(4, false, false)]
    [InlineData(3, true, false)]
    [InlineData(3, false, true)]
    [InlineData(2, true, true)]
    [InlineData(20, false, false)]
    [InlineData(20, true, false)]
    [InlineData(20, false, true)]
    [InlineData(20, true, true)]
    public void Factory_RepeatedGeneration_AlwaysSatisfiesClassGuarantees(int length, bool noNumbers,
        bool noSymbols)
    {
        for (int iteration = 0; iteration < 64; iteration++)
        {
            AssertPassword(PasswordFactory.New(length, noNumbers, noSymbols), length, noNumbers, noSymbols);
        }
    }

    [Theory]
    [InlineData(0, false, false)]
    [InlineData(-1, false, false)]
    [InlineData(3, false, false)]
    [InlineData(2, true, false)]
    [InlineData(2, false, true)]
    [InlineData(1, true, true)]
    [InlineData(1025, false, false)]
    [InlineData(1025, true, true)]
    [InlineData(int.MaxValue, false, false)]
    public void Factory_InvalidLength_ThrowsBeforeGeneration(int length, bool noNumbers, bool noSymbols) => _ = Throws<ArgumentOutOfRangeException>(() => PasswordFactory.New(length, noNumbers, noSymbols));

    [Theory]
    [InlineData("positive", "-l", "0")]
    [InlineData("positive", "--length", "-1")]
    [InlineData("at least 4", "-l", "3")]
    [InlineData("at least 3", "-l", "2", "-n")]
    [InlineData("at least 3", "-l", "2", "-s")]
    [InlineData("at least 2", "-l", "1", "-n", "-s")]
    [InlineData("1024", "--length", "1025")]
    [InlineData("1024", "-l", "2147483647")]
    public async Task Password_InvalidLength_PrintsOnlyStderrAndDoesNotCopy(string diagnostic,
        params string[] args)
    {
        CliResult result = await CliRunner.RunAsync(["password", "--copy", .. args]);

        Equal(1, result.ExitCode);
        Empty(result.StdOut);
        Contains(diagnostic, result.StdErr);
        DoesNotContain("Clipboard copy failed", result.StdErr);
    }

    [Theory]
    [InlineData("--length", "abc")]
    [InlineData("-l", "2147483648")]
    [InlineData("--length")]
    public async Task Password_InvalidLengthArgument_PreservesFrameworkDiagnostics(params string[] args)
    {
        CliResult result = await CliRunner.RunAsync(["password", "--copy", .. args]);

        Equal(1, result.ExitCode);
        NotEmpty(result.StdOut);
        Empty(result.StdErr);
        DoesNotContain("Clipboard copy failed", result.StdOut);
    }

    [Theory]
    [InlineData("password", "--help")]
    [InlineData("--copy", "password", "--help")]
    [InlineData("password", "--copy", "--help")]
    public async Task Password_Help_DescribesOptionsAndBoundsWithoutCopy(params string[] args)
    {
        CliResult result = await CliRunner.RunAsync(args);

        Equal(0, result.ExitCode);
        Empty(result.StdErr);
        foreach (string option in new[] { "--length", "-l", "--no-numbers", "-n", "--no-symbols", "-s", "--copy" })
        {
            Contains(option, result.StdOut);
        }

        Contains("20", result.StdOut);
        Contains("1024", result.StdOut);
        Contains("enabled", result.StdOut);
    }

    // Keep generated secrets out of assertion diagnostics, including when a test fails.
    internal static void AssertOutput(string output, int length, bool noNumbers, bool noSymbols)
    {
        output = output.Replace("\r\n", "\n");
        Equal(length + 1, output.Length);
        True(output.EndsWith('\n'), "Password output must end with one newline.");
        AssertPassword(output[..^1], length, noNumbers, noSymbols);
    }

    internal static void AssertPassword(string password, int length, bool noNumbers, bool noSymbols)
    {
        Equal(length, password.Length);
        string alphabet = Lowercase + Uppercase + (noNumbers ? "" : Numbers) + (noSymbols ? "" : Symbols);
        True(password.All(alphabet.Contains), "Password contains a character outside the approved alphabet.");
        True(password.Any(Lowercase.Contains), "Password must contain lowercase.");
        True(password.Any(Uppercase.Contains), "Password must contain uppercase.");
        True(noNumbers ? !password.Any(Numbers.Contains) : password.Any(Numbers.Contains),
            "Password must respect the enabled numbers class.");
        True(noSymbols ? !password.Any(Symbols.Contains) : password.Any(Symbols.Contains),
            "Password must respect the enabled symbols class.");
    }
}