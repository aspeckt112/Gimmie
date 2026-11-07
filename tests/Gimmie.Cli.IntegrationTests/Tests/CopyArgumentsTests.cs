using Gimmie.Cli.Output;

namespace Gimmie.Cli.IntegrationTests.Tests;

public sealed class CopyArgumentsTests
{
    [Fact]
    public void Copy_LeadingFlag_MovesBeforeEscapeWithoutChangingEscapedData()
    {
        string[] arguments = ["--copy", "guids", "-n", "0", "--", "--copy"];

        Equal(["guids", "-n", "0", "--copy", "--", "--copy"], CopyArguments.Normalize(arguments));
    }

    [Fact]
    public void Copy_EscapedFlag_IsNotAnOption()
    {
        string[] arguments = ["guid", "--", "--copy"];

        Same(arguments, CopyArguments.Normalize(arguments));
    }
}