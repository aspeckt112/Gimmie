using Gimmie.Core;
using Gimmie.Platform.Linux;
using Gimmie.Platform.MacOS;
using Gimmie.Platform.Windows;

namespace Gimmie.Cli.Output;

internal static class ClipboardFactory
{
    // Platform selection is centralized, so commands never depend on OS details.
    internal static IClipboard Create() => true switch
    {
        _ when OperatingSystem.IsWindows() => new WindowsClipboard(),
        _ when OperatingSystem.IsMacOS() => new MacOsClipboard(),
        _ when OperatingSystem.IsLinux() => new LinuxClipboard(),
        _ => new UnsupportedClipboard()
    };

    private sealed class UnsupportedClipboard : IClipboard
    {
        /// <inheritdoc />
        public Task CopyAsync(string text, CancellationToken cancellationToken = default) =>
            throw new PlatformNotSupportedException("Clipboard copying is supported on Windows, macOS, and Linux.");
    }
}