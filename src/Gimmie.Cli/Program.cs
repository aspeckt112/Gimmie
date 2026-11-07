using ConsoleAppFramework;

using Gimmie.Cli.Commands;
using Gimmie.Cli.Output;

ConsoleApp.ConsoleAppBuilder app = ConsoleApp.Create();

// Native global options provide shared parsing and generated command help.
app.ConfigureGlobalOptions((ref builder) =>
{
    bool copy = builder.AddGlobalOption<bool>("--copy", "Copy the response to the clipboard.");
    return new ResponseOptions(copy, ClipboardFactory.Create());
});

app.UseFilter<ResponseFilter>();

app.Add("guid", GuidCommand.Run);
app.Add("guids", GuidsCommand.Run);

await app.RunAsync(CopyArguments.Normalize(args));