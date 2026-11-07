using System.ComponentModel;

using ConsoleAppFramework;

using Gimmie.Core;

namespace Gimmie.Cli.Output;

internal sealed class CommandResponse
{
    internal string? Text { get; set; }

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
                throw new ClipboardCopyException(exception);
            }
        }
    }
}

internal sealed class ClipboardCopyException(Exception exception) : Exception(exception.Message, exception);

internal static class ResponseExtensions
{
    // All commands publish through the response state installed by the global filter.
    internal static void WriteResponse(this ConsoleAppContext context, string text) =>
        ((CommandResponse)context.State!).Text = text;
}