using System.ComponentModel;

using Gimmie.Core;

namespace Gimmie.Platform.Linux;

/// <summary>Copies text using clipboard tools available in the Linux desktop session.</summary>
public sealed class LinuxClipboard : IClipboard
{
    /// <inheritdoc />
    public Task CopyAsync(
        string text,
        CancellationToken cancellationToken = default) =>
        CopyAsync(
            text,
            Environment.GetEnvironmentVariable,
            ClipboardProcess.CopyAsync,
            cancellationToken);

    // Keep session selection testable without a real desktop session or clipboard.
    internal static async Task CopyAsync(
        string text,
        Func<string, string?> environment,
        Func<string, string[], string, CancellationToken, Task> copy,
        CancellationToken cancellationToken)
    {
        var failures = new List<string>();

        if (!string.IsNullOrEmpty(environment("WAYLAND_DISPLAY"))
            && await TryCopyAsync("wl-copy", ["--type", "text/plain;charset=utf-8"], text, failures, copy,
                cancellationToken))
        {
            return;
        }

        if (!string.IsNullOrEmpty(environment("DISPLAY")))
        {
            if (await TryCopyAsync("xclip", ["-selection", "clipboard", "-in"], text, failures, copy, cancellationToken)
                || await TryCopyAsync("xsel", ["--clipboard", "--input"], text, failures, copy, cancellationToken))
            {
                return;
            }
        }

        throw new IOException("No usable Linux clipboard. A desktop session and wl-copy (Wayland), "
                              + "xclip or xsel (X11) are required. " + string.Join(" ", failures));
    }

    // A failed Wayland tool may still permit a usable X11 fallback.
    private static async Task<bool> TryCopyAsync(
        string executable,
        string[] arguments,
        string text,
        List<string> failures,
        Func<string, string[], string, CancellationToken, Task> copy,
        CancellationToken cancellationToken)
    {
        try
        {
            await copy(executable, arguments, text, cancellationToken);
            return true;
        }
        catch (Exception exception) when (exception is Win32Exception or IOException)
        {
            failures.Add(exception.Message);
            return false;
        }
    }
}