using System.Security.Cryptography;

namespace Gimmie.Cli.Commands;

internal static class PasswordFactory
{
    internal const int DefaultLength = 20;

    private const int MaximumLength = 1024;
    private const string Lowercase = "abcdefghjkmnpqrstuvwxyz";
    private const string Uppercase = "ABCDEFGHJKMNPQRSTUVWXYZ";
    private const string Numbers = "23456789";
    private const string Symbols = "!#$%&()*+,-./:;<=>?@[]^_{|}~";

    internal static string? GetLengthError(int length, bool noNumbers, bool noSymbols)
    {
        int minimumLength = 2 + (noNumbers ? 0 : 1) + (noSymbols ? 0 : 1);

        return length switch
        {
            <= 0 => "Password length must be positive.",
            > MaximumLength => $"Password length must not exceed {MaximumLength} characters.",
            _ when length < minimumLength =>
                $"Password length must be at least {minimumLength} characters to include every enabled character class.",
            _ => null
        };
    }

    internal static string New(int length, bool noNumbers, bool noSymbols)
    {
        if (GetLengthError(length, noNumbers, noSymbols) is { } error)
        {
            throw new ArgumentOutOfRangeException(nameof(length), error);
        }

        string[] classes = (noNumbers, noSymbols) switch
        {
            (true, true) => [Lowercase, Uppercase],
            (true, false) => [Lowercase, Uppercase, Symbols],
            (false, true) => [Lowercase, Uppercase, Numbers],
            (false, false) => [Lowercase, Uppercase, Numbers, Symbols]
        };

        int alphabetLength = 0;

        foreach (string characterClass in classes)
        {
            alphabetLength += characterClass.Length;
        }

        int requiredClasses = (1 << classes.Length) - 1;
        Span<char> characters = stackalloc char[length];

        try
        {
            // Reject whole candidates so all passwords satisfying class coverage are equally likely.
            int seenClasses;

            do
            {
                seenClasses = 0;

                for (int position = 0; position < characters.Length; position++)
                {
                    int index = RandomNumberGenerator.GetInt32(alphabetLength);

                    for (int classIndex = 0; classIndex < classes.Length; classIndex++)
                    {
                        string characterClass = classes[classIndex];

                        if (index < characterClass.Length)
                        {
                            characters[position] = characterClass[index];
                            seenClasses |= 1 << classIndex;
                            break;
                        }

                        index -= characterClass.Length;
                    }
                }
            } while (seenClasses != requiredClasses);

            return new string(characters);
        }
        finally
        {
            characters.Clear();
        }
    }
}