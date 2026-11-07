using ConsoleAppFramework;

using Gimmie.Cli.Commands;
using Gimmie.Cli.Output;

namespace Gimmie.Cli.IntegrationTests.Tests;

[CollectionDefinition("Console response", DisableParallelization = true)]
public sealed class ConsoleResponseCollection;

[Collection("Console response")]
public sealed class ResponseFilterTests
{
    [Theory]
    [InlineData(false, 1, null)]
    [InlineData(true, 3, null)]
    [InlineData(true, 2, ';')]
    public async Task Filter_Commands_CopySameGeneratedResponse(bool upper, int count, char? separator)
    {
        var clipboard = new ResponseTests.RecordingClipboard();
        var context = new ConsoleAppContext("test", [], ReadOnlyMemory<string>.Empty,
            null, new ResponseOptions(true, clipboard), 0, -1);
        var filter = new ResponseFilter(new CommandFilter(commandContext =>
        {
            if (count == 1)
            {
                GuidCommand.Run(commandContext, upper);
            }
            else
            {
                GuidsCommand.Run(commandContext, count, upper, separator);
            }
        }));

        using var output = new StringWriter();
        TextWriter originalOutput = Console.Out;
        int originalExitCode = Environment.ExitCode;
        try
        {
            Environment.ExitCode = 0;
            Console.SetOut(output);
            await filter.InvokeAsync(context, TestContext.Current.CancellationToken);

            NotNull(clipboard.Text);
            Equal(clipboard.Text + Environment.NewLine, output.ToString());
            string[] values = clipboard.Text.Split(separator ?? '\n');
            Equal(count, values.Length);
            All(values, value =>
            {
                True(Guid.TryParse(value, out _));
                Equal(upper ? value.ToUpperInvariant() : value.ToLowerInvariant(), value);
            });
        }
        finally
        {
            Console.SetOut(originalOutput);
            Environment.ExitCode = originalExitCode;
        }
    }

    [Fact]
    public async Task Filter_FailedCommand_DoesNotCopyPartialResponse()
    {
        var clipboard = new ResponseTests.RecordingClipboard();
        var context = new ConsoleAppContext("test", [], ReadOnlyMemory<string>.Empty,
            null, new ResponseOptions(true, clipboard), 0, -1);
        var filter = new ResponseFilter(new CommandFilter(commandContext =>
        {
            commandContext.WriteResponse("partial");
            throw new InvalidOperationException("Command failed.");
        }));

        _ = await ThrowsAsync<InvalidOperationException>(() =>
            filter.InvokeAsync(context, TestContext.Current.CancellationToken));

        Null(clipboard.Text);
    }

    [Fact]
    public async Task Filter_ClipboardFailure_WritesStderrAndSetsNonzeroExit()
    {
        var context = new ConsoleAppContext("test", [], ReadOnlyMemory<string>.Empty,
            null, new ResponseOptions(true, new ResponseTests.FailingClipboard()), 0, -1);
        var filter = new ResponseFilter(new CommandFilter(commandContext => commandContext.WriteResponse("result")));
        using var output = new StringWriter();
        using var error = new StringWriter();
        TextWriter originalOutput = Console.Out;
        TextWriter originalError = Console.Error;
        int originalExitCode = Environment.ExitCode;
        try
        {
            Environment.ExitCode = 0;
            Console.SetOut(output);
            Console.SetError(error);
            await filter.InvokeAsync(context, TestContext.Current.CancellationToken);

            Equal(1, Environment.ExitCode);
            Equal("result" + Environment.NewLine, output.ToString());
            Contains("Clipboard copy failed: Clipboard unavailable.", error.ToString());
        }
        finally
        {
            Console.SetOut(originalOutput);
            Console.SetError(originalError);
            Environment.ExitCode = originalExitCode;
        }
    }

    private sealed class CommandFilter(Action<ConsoleAppContext> command) : ConsoleAppFilter(null!)
    {
        // Invoke real command code without framework routing or a desktop clipboard.
        public override Task InvokeAsync(ConsoleAppContext context, CancellationToken cancellationToken)
        {
            command(context);
            return Task.CompletedTask;
        }
    }
}