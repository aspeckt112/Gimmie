using Gimmie.Cli.Commands;

namespace Gimmie.Cli.IntegrationTests.Tests;

public sealed class RandomSelectionTests
{
    [Theory]
    [InlineData(int.MinValue, int.MaxValue, 0u, int.MinValue)]
    [InlineData(int.MinValue, int.MaxValue, uint.MaxValue, int.MaxValue)]
    [InlineData(-5, 5, 0u, -5)]
    [InlineData(-5, 5, 10u, 5)]
    [InlineData(int.MaxValue - 1, int.MaxValue, 1u, int.MaxValue)]
    public void Integer_MapsSamplesToInclusiveBounds(int min, int max, uint sample, int expected) => Equal(expected, RandomSelection.Integer(min, max, () => sample));

    [Fact]
    public void Integer_EqualBounds_DoNotConsumeRandomness() => Equal(-8, RandomSelection.Integer(-8, -8, () => throw new InvalidOperationException()));

    [Fact]
    public void Integer_RejectsIncompleteHighTail()
    {
        var samples = new Queue<uint>([uint.MaxValue, 4]);

        Equal(1, RandomSelection.Integer(0, 2, samples.Dequeue));
        Empty(samples);
    }

    [Fact]
    public void Integer_AlmostFullRange_RejectsOnlyLastSample()
    {
        var samples = new Queue<uint>([uint.MaxValue, uint.MaxValue - 1]);

        Equal(int.MaxValue - 1, RandomSelection.Integer(int.MinValue, int.MaxValue - 1, samples.Dequeue));
        Empty(samples);
    }

    [Fact]
    public void Integer_ReversedBounds_Throws() => Throws<ArgumentOutOfRangeException>(() => RandomSelection.Integer(1, 0, () => 0));

    [Fact]
    public void Choice_DuplicatesRemainSeparatePositions()
    {
        string[] choices = ["repeat", "other", "repeat"];

        Equal("repeat", RandomSelection.Choice(choices, () => 0));
        Equal("other", RandomSelection.Choice(choices, () => 1));
        Equal("repeat", RandomSelection.Choice(choices, () => 2));
    }
}