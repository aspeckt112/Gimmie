using System.Runtime.Versioning;

using Gimmie.Core;

namespace Gimmie.Platform.MacOS;

/// <summary>Copies text using macOS's pbcopy tool.</summary>
[SupportedOSPlatform("macos")]
public sealed class MacOsClipboard : IClipboard
{
    /// <inheritdoc />
    public Task CopyAsync(
        string text,
        CancellationToken cancellationToken = default) =>
            ClipboardProcess.CopyAsync(
                "pbcopy",
                [],
                text,
                cancellationToken);
}