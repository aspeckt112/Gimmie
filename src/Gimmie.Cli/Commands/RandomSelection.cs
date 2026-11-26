using System.Security.Cryptography;

namespace Gimmie.Cli.Commands;

internal static class RandomSelection
{
    internal static int Integer(int min, int max) => Integer(min, max, NextUInt32);

    internal static int Integer(int min, int max, Func<uint> next)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(min, max);

        ulong width = (ulong)((long)max - min) + 1;

        if (width == 1)
        {
            return min;
        }

        const ulong sampleSpace = 1UL << 32;
        // Discard the incomplete high tail to give each result the same number of samples.
        ulong limit = sampleSpace - sampleSpace % width;
        uint sample;

        do
        {
            sample = next();
        } while (sample >= limit);

        return (int)(min + (long)(sample % width));
    }

    internal static string Choice(IReadOnlyList<string> choices, Func<uint> next)
    {
        ArgumentOutOfRangeException.ThrowIfZero(choices.Count);
        return choices[Integer(0, choices.Count - 1, next)];
    }

    internal static string Choice(IReadOnlyList<string> choices) => Choice(choices, NextUInt32);

    private static uint NextUInt32()
    {
        Span<byte> bytes = stackalloc byte[sizeof(uint)];
        RandomNumberGenerator.Fill(bytes);
        return BitConverter.ToUInt32(bytes);
    }
}