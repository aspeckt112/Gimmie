using ConsoleAppFramework;

using Gimmie.Cli.Commands;
using Gimmie.Cli.Output;
using Gimmie.Core;

namespace Gimmie.Cli.IntegrationTests.Tests;

[Collection("Console response")]
public sealed class PasswordResponseTests
{
    [Theory]
    [InlineData(false, "password")]
    [InlineData(true, "--copy", "password")]
    [InlineData(true, "password", "--copy")]
    public async Task Password_Copy_OnlyCopiesWhenExplicitlyRequested(bool copy, params string[] args)
    {
        var clipboard = new ResponseTests.RecordingClipboard();
        (string output, string error, int exitCode) = await RunAsync(clipboard, args);

        Equal(0, exitCode);
        Empty(error);
        PasswordCommandTests.AssertOutput(output, 20, false, false);
        if (copy)
        {
            True(clipboard.Text == output[..^Environment.NewLine.Length],
                "Clipboard must receive the same password without the display newline.");
        }
        else
        {
            Null(clipboard.Text);
        }
    }

    [Theory]
    [InlineData("password", "--copy", "-l", "0")]
    [InlineData("--copy", "password", "-l", "3")]
    [InlineData("password", "--copy", "-l", "1025")]
    public async Task Password_InvalidLength_DoesNotAccessClipboard(params string[] args)
    {
        var clipboard = new ResponseTests.RecordingClipboard();
        (string output, string error, int exitCode) = await RunAsync(clipboard, args);

        Equal(1, exitCode);
        Empty(output);
        NotEmpty(error);
        Null(clipboard.Text);
    }

    [Fact]
    public async Task Password_ClipboardFailure_RedactsSecretAndPreservesStdout()
    {
        (string output, string error, int exitCode) =
            await RunAsync(new EchoingFailureClipboard(), ["password", "--copy"]);

        Equal(1, exitCode);
        PasswordCommandTests.AssertOutput(output, 20, false, false);
        Equal("Clipboard copy failed: Unable to copy password." + Environment.NewLine, error);
    }

    [Fact]
    public async Task SensitiveResponse_ClipboardFailure_DoesNotRetainOriginalException()
    {
        var response = new CommandResponse { Text = "sensitive test value", Sensitive = true };
        using var output = new StringWriter();

        ClipboardCopyException exception = await ThrowsAsync<ClipboardCopyException>(() =>
            response.PublishAsync(output, true, new EchoingFailureClipboard(),
                TestContext.Current.CancellationToken));

        Equal("Unable to copy password.", exception.Message);
        Null(exception.InnerException);
    }

    private static async Task<(string Output, string Error, int ExitCode)> RunAsync(IClipboard clipboard, string[] args)
    {
        ConsoleApp.ConsoleAppBuilder app = ConsoleApp.Create();
        _ = app.ConfigureGlobalOptions((ref builder) =>
        {
            bool copy = builder.AddGlobalOption<bool>("--copy", "Copy the response to the clipboard.");
            return new ResponseOptions(copy, clipboard);
        });
        app.UseFilter<ResponseFilter>();
        app.Add("password", PasswordCommand.Run);

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

    private sealed class EchoingFailureClipboard : IClipboard
    {
        /// <inheritdoc />
        public Task CopyAsync(string text, CancellationToken cancellationToken = default) => throw new IOException(text);
    }
}