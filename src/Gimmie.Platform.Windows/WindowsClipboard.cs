using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;

using Gimmie.Core;

namespace Gimmie.Platform.Windows;

/// <summary>Copies Unicode text using the Windows clipboard without a UI framework.</summary>
[SupportedOSPlatform("windows")]
public sealed partial class WindowsClipboard : IClipboard
{
    /// <inheritdoc />
    public Task CopyAsync(string text, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        // EmptyClipboard needs a real owner; opening with a null owner prevents SetClipboardData.
        nint owner = CreateWindowExW(0, "STATIC", "GimmieClipboard", 0, 0, 0, 0, 0, -3, 0, 0, 0);

        if (owner == 0)
        {
            throw new Win32Exception(Marshal.GetLastPInvokeError(), "Cannot create a clipboard owner.");
        }

        bool opened = false;
        nint memory = 0;
        try
        {
            for (int attempt = 0; attempt < 10; attempt++)
            {
                if (OpenClipboard(owner) != 0)
                {
                    opened = true;
                    break;
                }

                // Keep all window and clipboard calls on the owner window's thread.
                _ = cancellationToken.WaitHandle.WaitOne(50);
                cancellationToken.ThrowIfCancellationRequested();
            }

            if (!opened)
            {
                throw new IOException("Windows clipboard is busy. Try copying again.");
            }

            byte[] bytes = Encoding.Unicode.GetBytes(text + '\0');
            memory = GlobalAlloc(0x0002, (nuint)bytes.Length);
            if (memory == 0)
            {
                throw new Win32Exception(Marshal.GetLastPInvokeError(), "Cannot allocate clipboard memory.");
            }

            nint pointer = GlobalLock(memory);
            if (pointer == 0)
            {
                throw new Win32Exception(Marshal.GetLastPInvokeError(), "Cannot lock clipboard memory.");
            }

            try
            {
                Marshal.Copy(bytes, 0, pointer, bytes.Length);
            }
            finally
            {
                _ = GlobalUnlock(memory);
            }

            if (EmptyClipboard() == 0 || SetClipboardData(13, memory) == 0)
            {
                throw new Win32Exception(Marshal.GetLastPInvokeError(), "Cannot set Windows clipboard text.");
            }

            // Successful SetClipboardData transfers ownership of the memory to Windows.
            memory = 0;
        }
        finally
        {
            if (memory != 0)
            {
                _ = GlobalFree(memory);
            }

            if (opened)
            {
                _ = CloseClipboard();
            }

            _ = DestroyWindow(owner);
        }

        return Task.CompletedTask;
    }

    [LibraryImport("user32.dll", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
    private static partial nint CreateWindowExW(uint extendedStyle, string className, string windowName,
        uint style, int x, int y, int width, int height, nint parent, nint menu, nint instance, nint parameter);

    [LibraryImport("user32.dll", SetLastError = true)]
    private static partial int OpenClipboard(nint owner);

    [LibraryImport("user32.dll", SetLastError = true)]
    private static partial int EmptyClipboard();

    [LibraryImport("user32.dll", SetLastError = true)]
    private static partial nint SetClipboardData(uint format, nint memory);

    [LibraryImport("user32.dll")]
    private static partial int CloseClipboard();

    [LibraryImport("user32.dll")]
    private static partial int DestroyWindow(nint window);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    private static partial nint GlobalAlloc(uint flags, nuint bytes);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    private static partial nint GlobalLock(nint memory);

    [LibraryImport("kernel32.dll")]
    private static partial int GlobalUnlock(nint memory);

    [LibraryImport("kernel32.dll")]
    private static partial nint GlobalFree(nint memory);
}