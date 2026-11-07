namespace Gimmie.Cli.Output;

internal static class CopyArguments
{
    // Framework 5.7.13 routes command names before parsing global options.
    // Move only leading --copy flags behind the command, never across an escape separator.
    internal static string[] Normalize(string[] arguments)
    {
        int leadingCopies = 0;

        while (leadingCopies < arguments.Length && arguments[leadingCopies] == "--copy")
        {
            leadingCopies++;
        }

        if (leadingCopies == 0 || leadingCopies == arguments.Length)
        {
            return arguments;
        }

        int escapeIndex = Array.IndexOf(arguments, "--", leadingCopies);

        if (escapeIndex < 0)
        {
            escapeIndex = arguments.Length;
        }

        return
        [
            .. arguments[leadingCopies..escapeIndex],
            .. arguments[..leadingCopies], .. arguments[escapeIndex..]
        ];
    }
}