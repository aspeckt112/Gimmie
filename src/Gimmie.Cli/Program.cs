using ConsoleAppFramework;

using Gimmie.Cli.Commands;

ConsoleApp.ConsoleAppBuilder app = ConsoleApp.Create();

app.Add("guid", GuidCommand.Run);
app.Add("guids", GuidsCommand.Run);

await app.RunAsync(args);