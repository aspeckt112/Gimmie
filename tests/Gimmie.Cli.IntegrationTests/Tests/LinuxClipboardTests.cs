using System.ComponentModel;

using Gimmie.Platform.Linux;

namespace Gimmie.Cli.IntegrationTests.Tests;

public sealed class LinuxClipboardTests
{
    [Fact]
    public async Task Linux_Wayland_CopiesUtf8TextWithoutTryingX11()
    {
        var tools = new List<string>();

        await LinuxClipboard.CopyAsync("response", _ => "session", (tool, arguments, text, token) =>
        {
            tools.Add(tool);
            Equal(["--type", "text/plain;charset=utf-8"], arguments);
            Equal("response", text);
            return Task.CompletedTask;
        }, TestContext.Current.CancellationToken);

        Equal(["wl-copy"], tools);
    }

    [Fact]
    public async Task Linux_MissingTools_FallsBackFromWaylandToX11Clipboard()
    {
        var tools = new List<string>();

        await LinuxClipboard.CopyAsync("response", _ => "session", (tool, arguments, text, token) =>
        {
            tools.Add(tool);

            if (tool != "xsel")
            {
                throw new Win32Exception("Tool missing.");
            }

            Equal(["--clipboard", "--input"], arguments);
            Equal("response", text);
            return Task.CompletedTask;
        }, TestContext.Current.CancellationToken);

        Equal(["wl-copy", "xclip", "xsel"], tools);
    }

    [Fact]
    public async Task Linux_Headless_ReportsSessionRequirementWithoutStartingTools()
    {
        IOException error = await ThrowsAsync<IOException>(() => LinuxClipboard.CopyAsync("response", _ => null,
            (_, _, _, _) => throw new InvalidOperationException("Must not start a tool."),
            TestContext.Current.CancellationToken));

        Contains("desktop session", error.Message);
    }

    [Fact]
    public async Task Linux_UnavailableX11Tools_ReportsInstallableToolNames()
    {
        IOException error = await ThrowsAsync<IOException>(() => LinuxClipboard.CopyAsync("response",
            name => name == "DISPLAY" ? ":0" : null,
            (_, _, _, _) => throw new Win32Exception("Tool missing."),
            TestContext.Current.CancellationToken));

        Contains("xclip or xsel", error.Message);
        Contains("Tool missing.", error.Message);
    }
}