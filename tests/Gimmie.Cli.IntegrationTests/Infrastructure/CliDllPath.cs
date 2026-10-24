using System.Reflection;

namespace Gimmie.Cli.IntegrationTests.Infrastructure;

/// <summary>Locates the Gimmie.Cli build output injected as assembly metadata.</summary>
internal static class CliDllPath
{
    public static string Resolve()
    {
        string? path = typeof(CliDllPath).Assembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .SingleOrDefault(a => a.Key == "GimmieCliPath")
            ?.Value;

        if (string.IsNullOrEmpty(path))
        {
            throw new InvalidOperationException(
                "Assembly metadata 'GimmieCliPath' was not found. Build the test project before running it.");
        }

        if (!File.Exists(path))
        {
            throw new FileNotFoundException(
                $"The Gimmie.Cli build output was not found at '{path}'. Build the Gimmie.Cli project first.",
                path);
        }

        return path;
    }
}
