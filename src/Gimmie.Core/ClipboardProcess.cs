using System.Diagnostics;
using System.Text;

namespace Gimmie.Core;

/// <summary>Writes clipboard text to an OS tool without invoking a shell.</summary>
public static class ClipboardProcess
{
    /// <summary>Runs a clipboard tool with redirected UTF-8 input and a bounded wait.</summary>
    public static async Task CopyAsync(
        string executable,
        string[] arguments,
        string text,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        timeout.CancelAfter(TimeSpan.FromSeconds(10));

        using var process = new Process();

        process.StartInfo = new ProcessStartInfo(executable)
        {
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            StandardInputEncoding = new UTF8Encoding(false),
            StandardErrorEncoding = Encoding.UTF8,
            CreateNoWindow = true
        };

        foreach (string argument in arguments)
        {
            process.StartInfo.ArgumentList.Add(argument);
        }

        _ = process.Start();
        var diagnostic = new StringBuilder();

        Task error = DrainAsync(process.StandardError, diagnostic, timeout.Token);
        Task output = DrainAsync(process.StandardOutput, null, timeout.Token);

        try
        {
            await process.StandardInput.WriteAsync(text.AsMemory(), timeout.Token);
            process.StandardInput.Close();
            await process.WaitForExitAsync(timeout.Token);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new IOException($"{executable} timed out while accessing the clipboard.");
        }
        finally
        {
            if (!process.HasExited)
            {
                process.Kill(true);
                await process.WaitForExitAsync(CancellationToken.None);
            }

            // wl-copy deliberately leaves stderr open in its clipboard-owner child.
            // Parent exit, not pipe EOF, defines completion of a clipboard request.
            await timeout.CancelAsync();
            await Task.WhenAll(error, output);
        }

        if (process.ExitCode != 0)
        {
            throw new IOException($"{executable} failed (exit {process.ExitCode}): {diagnostic.ToString().Trim()}");
        }
    }

    // Drain helper output without exposing it on the command's stdout.
    private static async Task DrainAsync(
        StreamReader reader,
        StringBuilder? diagnostic,
        CancellationToken cancellationToken)
    {
        char[] buffer = new char[1024];

        try
        {
            int count;

            while ((count = await reader.ReadAsync(buffer.AsMemory(), cancellationToken)) != 0)
            {
                _ = diagnostic?.Append(buffer, 0, count);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
    }
}