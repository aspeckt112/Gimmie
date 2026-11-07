namespace Gimmie.Core;

/// <summary>Copies text to the current desktop session's clipboard.</summary>
public interface IClipboard
{
    /// <summary>Replaces clipboard text, or throws if copying is unavailable.</summary>
    Task CopyAsync(string text, CancellationToken cancellationToken = default);
}