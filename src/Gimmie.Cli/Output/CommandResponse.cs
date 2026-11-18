using System.ComponentModel;

using ConsoleAppFramework;

using Gimmie.Core;

namespace Gimmie.Cli.Output;

internal sealed class CommandResponse
{
    internal string? Text { get; set; }
    internal bool Sensitive { get; set; }

    // Display termination is not part of the clipboard payload.
    internal async Task PublishAsync(TextWriter output, bool copy, IClipboard clipboard,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(Text))
        {
            return;
        }

        await output.WriteLineAsync(Text);

        if (copy)
        {
            try
            {
                await clipboard.CopyAsync(Text, cancellationToken);
            }
            catch (Exception exception) when (exception is Win32Exception or IOException
                                                  or PlatformNotSupportedException or OperationCanceledException)
            {
                throw new ClipboardCopyException(exception, Sensitive);
            }
        }
    }
}

internal sealed class ClipboardCopyException(Exception exception, bool sensitive = false)
    : Exception(sensitive ? "Unable to copy password." : exception.Message, sensitive ? null : exception);

internal static class ResponseExtensions
{
    // All commands publish through the response state installed by the global filter.
    internal static void WriteResponse(this ConsoleAppContext context, string text, bool sensitive = false)
    {
        var response = (CommandResponse)context.State!;
        response.Text = text;
        response.Sensitive = sensitive;
    }
}