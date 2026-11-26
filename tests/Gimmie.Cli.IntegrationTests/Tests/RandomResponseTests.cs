using ConsoleAppFramework;

using Gimmie.Cli.Commands;
using Gimmie.Cli.Output;

namespace Gimmie.Cli.IntegrationTests.Tests;

[Collection("Console response")]
public sealed class RandomResponseTests
{
    [Theory]
    [InlineData(false, "random", "integer", "--min", "-2", "--max", "-2")]
    [InlineData(true, "--copy", "random", "integer", "--min", "-2", "--max", "-2")]
    [InlineData(true, "random", "integer", "--min", "-2", "--max", "-2", "--copy")]
    [InlineData(false, "random", "choice", "  choice  ")]
    [InlineData(true, "--copy", "random", "choice", "  choice  ")]
    [InlineData(true, "random", "choice", "  choice  ", "--copy")]
    [InlineData(false, "random", "choice", "--", "--copy")]
    [InlineData(true, "random", "choice", "--copy", "--", "--copy")]
    public async Task Random_Copy_OnlyCopiesExplicitlyWithoutDisplayNewline(bool copy, params string[] args)
    {
        var clipboard = new ResponseTests.RecordingClipboard();
        (string output, string error, int exitCode) = await RunAsync(clipboard, args);

        Equal(0, exitCode);
        Empty(error);
        string expected = args.Contains("integer") ? "-2" :
            args.Contains("  choice  ") ? "  choice  " : "--copy";
        Equal(expected + Environment.NewLine, output);
        Equal(copy ? expected : null, clipboard.Text);
    }

    [Theory]
    [InlineData("random", "integer", "--copy", "--min", "5", "--max", "4")]
    [InlineData("random", "choice", "--copy", "")]
    public async Task Random_InvalidInput_DoesNotAccessClipboard(params string[] args)
    {
        var clipboard = new ResponseTests.RecordingClipboard();
        (string output, string error, int exitCode) = await RunAsync(clipboard, args);

        NotEqual(0, exitCode);
        Empty(output);
        NotEmpty(error);
        Null(clipboard.Text);
    }

    private static async Task<(string Output, string Error, int ExitCode)> RunAsync(
        ResponseTests.RecordingClipboard clipboard, string[] args)
    {
        ConsoleApp.ConsoleAppBuilder app = ConsoleApp.Create();
        _ = app.ConfigureGlobalOptions((ref builder) =>
        {
            bool copy = builder.AddGlobalOption<bool>("--copy", "Copy the response to the clipboard.");
            return new ResponseOptions(copy, clipboard);
        });
        app.UseFilter<ResponseFilter>();
        app.Add("random integer", RandomIntegerCommand.Run);
        app.Add("random choice", RandomChoiceCommand.Run);

        using var output = new StringWriter();
        using var error = new StringWriter();
        TextWriter originalOutput = Console.Out;
        TextWriter originalError = Console.Error;
        int originalExitCode = Environment.ExitCode;
        try
        {
            Environment.ExitCode = 0;
            Console.SetOut(output);
            Console.SetError(error);
            await app.RunAsync(CopyArguments.Normalize(args));

            return (output.ToString(), error.ToString(), Environment.ExitCode);
        }
        finally
        {
            Console.SetOut(originalOutput);
            Console.SetError(originalError);
            Environment.ExitCode = originalExitCode;
        }
    }
}