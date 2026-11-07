namespace Gimmie.Cli.IntegrationTests.Tests;

public sealed class ClipboardIntegrationTests
{
    [Theory]
    [InlineData("--copy", "guid", "-u")]
    [InlineData("guids", "--copy", "-n", "3", "-s", ";")]
    public async Task Copy_RealCli_CopiesPrintedResponseWithoutDisplayNewline(params string[] args)
    {
        if (OperatingSystem.IsWindows())
        {
            Skip("Windows uses native clipboard calls; response-module tests use a fake adapter instead.");
            return;
        }

        string directory = Path.Combine(Path.GetTempPath(), "gimmie-clipboard-" + Guid.NewGuid());
        _ = Directory.CreateDirectory(directory);

        string tool = Path.Combine(directory, OperatingSystem.IsMacOS() ? "pbcopy" : "wl-copy");
        string clipboardPath = Path.Combine(directory, "clipboard.txt");

        try
        {
            await File.WriteAllTextAsync(
                tool,
                "#!/bin/sh\n/bin/cat > \"$GIMMIE_TEST_CLIPBOARD\"\nprintf 'tool stdout must not leak'\n",
                TestContext.Current.CancellationToken);

            File.SetUnixFileMode(tool, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);

            var environment = new Dictionary<string, string?>
            {
                ["PATH"] = directory + Path.PathSeparator + Environment.GetEnvironmentVariable("PATH"),
                ["WAYLAND_DISPLAY"] = "gimmie-test",
                ["GIMMIE_TEST_CLIPBOARD"] = clipboardPath
            };

            CliResult result = await CliRunner.RunWithEnvironmentAsync(environment, args);

            Equal(0, result.ExitCode);
            Empty(result.StdErr);
            string text = await File.ReadAllTextAsync(clipboardPath, TestContext.Current.CancellationToken);
            Equal(text + "\n", result.StdOut);
            DoesNotContain("\n", text);
            DoesNotContain("tool stdout", result.StdOut);
            All(text.Split(';'), value => True(Guid.TryParse(value, out _)));
        }
        finally
        {
            File.Delete(tool);
            File.Delete(clipboardPath);
            Directory.Delete(directory);
        }
    }

    [Fact]
    public async Task Copy_RealCli_ToolFailureRetainsStdoutAndExitsNonzero()
    {
        if (OperatingSystem.IsWindows())
        {
            Skip("Windows uses native clipboard calls; response-module tests use a fake adapter instead.");
            return;
        }

        string directory = Path.Combine(Path.GetTempPath(), "gimmie-clipboard-" + Guid.NewGuid());
        _ = Directory.CreateDirectory(directory);
        string tool = Path.Combine(directory, OperatingSystem.IsMacOS() ? "pbcopy" : "wl-copy");

        try
        {
            await File.WriteAllTextAsync(
                tool,
                "#!/bin/sh\n/bin/cat >/dev/null\nprintf 'clipboard unavailable' >&2\nexit 7\n",
                TestContext.Current.CancellationToken);

            File.SetUnixFileMode(tool, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);

            var environment = new Dictionary<string, string?>
            {
                ["PATH"] = directory,
                ["WAYLAND_DISPLAY"] = "gimmie-test",
                ["DISPLAY"] = null
            };

            CliResult result = await CliRunner.RunWithEnvironmentAsync(environment, "guid", "--copy");

            Equal(1, result.ExitCode);
            Matches(GuidPatterns.Single(), Single(result.StdOutLines));
            Contains("Clipboard copy failed:", result.StdErr);
            Contains("clipboard unavailable", result.StdErr);
        }
        finally
        {
            File.Delete(tool);
            Directory.Delete(directory);
        }
    }
}