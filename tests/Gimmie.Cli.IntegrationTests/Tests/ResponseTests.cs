using Gimmie.Cli.Output;
using Gimmie.Core;

namespace Gimmie.Cli.IntegrationTests.Tests;

public sealed class ResponseTests
{
    [Theory]
    [InlineData("one")]
    [InlineData("one\ntwo")]
    [InlineData("one;two")]
    [InlineData("one \n")]
    [InlineData("\u03bb\uD83D\uDE80")]
    public async Task Response_Copy_PrintsDisplayNewlineButCopiesOriginalContent(string text)
    {
        var clipboard = new RecordingClipboard();
        using var output = new StringWriter();
        var response = new CommandResponse { Text = text };

        await response.PublishAsync(output, true, clipboard, TestContext.Current.CancellationToken);

        Equal(text + Environment.NewLine, output.ToString());
        Equal(text, clipboard.Text);
    }

    [Fact]
    public async Task Response_Default_PrintsWithoutAccessingClipboard()
    {
        var clipboard = new RecordingClipboard();
        using var output = new StringWriter();

        await new CommandResponse { Text = "result" }.PublishAsync(output, false, clipboard,
            TestContext.Current.CancellationToken);

        Equal("result" + Environment.NewLine, output.ToString());
        Null(clipboard.Text);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Response_Empty_DoesNotPrintOrReplaceClipboard(string? text)
    {
        var clipboard = new RecordingClipboard();
        using var output = new StringWriter();

        await new CommandResponse { Text = text }.PublishAsync(output, true, clipboard,
            TestContext.Current.CancellationToken);

        Empty(output.ToString());
        Null(clipboard.Text);
    }

    [Fact]
    public async Task Response_CopyFailure_PreservesPrintedResponse()
    {
        using var output = new StringWriter();

        _ = await ThrowsAsync<ClipboardCopyException>(() =>
            new CommandResponse { Text = "result" }.PublishAsync(output, true, new FailingClipboard(),
                TestContext.Current.CancellationToken));

        Equal("result" + Environment.NewLine, output.ToString());
    }

    [Fact]
    public async Task Response_OutputFailure_IsNotReportedAsClipboardFailure()
    {
        var clipboard = new RecordingClipboard();
        using var output = new FailingWriter();

        _ = await ThrowsAsync<IOException>(() =>
            new CommandResponse { Text = "result" }.PublishAsync(output, true, clipboard,
                TestContext.Current.CancellationToken));

        Null(clipboard.Text);
    }

    private sealed class FailingWriter : StringWriter
    {
        /// <inheritdoc />
        public override Task WriteLineAsync(string? value) => throw new IOException("Output failed.");
    }

    internal sealed class RecordingClipboard : IClipboard
    {
        internal string? Text { get; private set; }

        /// <inheritdoc />
        public Task CopyAsync(string text, CancellationToken cancellationToken = default)
        {
            Text = text;
            return Task.CompletedTask;
        }
    }

    internal sealed class FailingClipboard : IClipboard
    {
        /// <inheritdoc />
        public Task CopyAsync(string text, CancellationToken cancellationToken = default) => throw new IOException("Clipboard unavailable.");
    }
}