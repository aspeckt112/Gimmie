using System.ComponentModel;
using System.Diagnostics;
using System.Text;

using Gimmie.Core;

namespace Gimmie.Cli.IntegrationTests.Tests;

public sealed class ClipboardProcessTests
{
    [Fact]
    public async Task Process_Copy_WritesExactUtf8Input()
    {
        SkipWhen(OperatingSystem.IsWindows(), "This fake clipboard tool uses a POSIX shell.");

        string path = Path.GetTempFileName();
        const string text = "\u03bb\uD83D\uDE80\nsecond line ";

        try
        {
            await ClipboardProcess.CopyAsync("/bin/sh", ["-c", "cat > \"$1\"", "clipboard-test", path],
                text, TestContext.Current.CancellationToken);

            Equal(Encoding.UTF8.GetBytes(text), await File.ReadAllBytesAsync(path, TestContext.Current.CancellationToken));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task Process_FailedTool_ReportsExitCodeAndDiagnostic()
    {
        SkipWhen(OperatingSystem.IsWindows(), "This fake clipboard tool uses a POSIX shell.");

        IOException error = await ThrowsAsync<IOException>(() => ClipboardProcess.CopyAsync(
            "/bin/sh",
            ["-c", "cat >/dev/null; printf 'clipboard unavailable' >&2; exit 7"],
            "result",
            TestContext.Current.CancellationToken));

        Contains("exit 7", error.Message);
        Contains("clipboard unavailable", error.Message);
    }

    [Fact]
    public async Task Process_EarlyToolExit_ReportsExitCodeAndDiagnostic()
    {
        SkipWhen(OperatingSystem.IsWindows(), "This fake clipboard tool uses a POSIX shell.");
        string text = new('x', 1024 * 1024);

        IOException error = await ThrowsAsync<IOException>(() => ClipboardProcess.CopyAsync(
            "/bin/sh",
            ["-c", "exec 0<&-; printf 'clipboard unavailable' >&2; exit 7"],
            text,
            TestContext.Current.CancellationToken));

        Contains("exit 7", error.Message);
        Contains("clipboard unavailable", error.Message);
    }

    [Fact]
    public async Task Process_EarlySuccessfulToolExit_ReportsInputFailure()
    {
        SkipWhen(OperatingSystem.IsWindows(), "This fake clipboard tool uses a POSIX shell.");
        string text = new('x', 1024 * 1024);

        _ = await ThrowsAsync<IOException>(() => ClipboardProcess.CopyAsync(
            "/bin/sh",
            ["-c", "exec 0<&-; exit 0"],
            text,
            TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Process_MissingTool_FailsWithoutReportingSuccess()
    {
        string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        _ = await ThrowsAsync<Win32Exception>(() =>
            ClipboardProcess.CopyAsync(path, [], "result", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Process_CanceledRequest_DoesNotStartTool()
    {
        using var cancellation = new CancellationTokenSource();

        await cancellation.CancelAsync();

        _ = await ThrowsAnyAsync<OperationCanceledException>(() => ClipboardProcess.CopyAsync(
            Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()), [], "result", cancellation.Token));
    }

    [Fact]
    public async Task Process_ForkedClipboardOwner_DoesNotWaitForPipeEof()
    {
        SkipWhen(OperatingSystem.IsWindows(), "This fake clipboard owner uses a POSIX shell.");
        string path = Path.GetTempFileName();
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(5));

        try
        {
            await ClipboardProcess.CopyAsync("/bin/sh",
                ["-c", "cat >/dev/null; sleep 30 & echo $! > \"$1\"", "clipboard-test", path],
                "result", timeout.Token);
        }
        finally
        {
            string pidText = await File.ReadAllTextAsync(path, CancellationToken.None);

            if (int.TryParse(pidText.Trim(), out int pid))
            {
                using var child = Process.GetProcessById(pid);
                if (!child.HasExited)
                {
                    child.Kill();
                }
            }

            File.Delete(path);
        }
    }
}