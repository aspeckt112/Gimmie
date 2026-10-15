using ConsoleAppFramework;
using Gimmie.Cli;

ConsoleApp.ConsoleAppBuilder app = ConsoleApp.Create();

app.Add("guid", GuidCommand.Run);

await app.RunAsync(args);
