using ConsoleAppFramework;

namespace Gimmie.Cli.Output;

internal sealed class ResponseFilter(ConsoleAppFilter next) : ConsoleAppFilter(next)
{
    // The framework invokes filters only for routed commands, not help/version output.
    public override async Task InvokeAsync(ConsoleAppContext context, CancellationToken cancellationToken)
    {
        var response = new CommandResponse();

        await Next.InvokeAsync(context with { State = response }, cancellationToken);

        if (Environment.ExitCode != 0)
        {
            return;
        }

        var options = (ResponseOptions)context.GlobalOptions!;

        try
        {
            await response.PublishAsync(Console.Out, options.Copy, options.Clipboard, cancellationToken);
        }
        catch (ClipboardCopyException exception)
        {
            await Console.Error.WriteLineAsync($"Clipboard copy failed: {exception.Message}");

            Environment.ExitCode = 1;
        }
    }
}