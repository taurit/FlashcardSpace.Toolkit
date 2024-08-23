using CoreLibrary.Utilities;
using FluentAssertions;

namespace CoreLibrary.Tests.Utilities;

[TestClass]
public class SubtitlesHelperTests
{
    [TestMethod]
    public void When_SubtitlesArePreprocessed_Expect_SequenceNumberIsIgnored()
    {
        // Arrange
        var subtitles = @"1
            00:00:00,000 --> 00:00:02,000
            Hello
            
            2
            00:00:02,000 --> 00:00:04,000
            World";

        // Act
        var text = SubtitlesHelperSrt.ConvertSubtitlesToText(subtitles);

        // Assert
        text.Should().NotContain("1");
    }

    [TestMethod]
    public void When_SubtitlesArePreprocessed_Expect_TimestampLinesAreNotPreserved()
    {
        // Arrange
        var subtitles = @"1
            00:00:00,000 --> 00:00:02,000
            Hello
            
            2
            00:00:02,000 --> 00:00:04,000
            World";

        // Act
        var text = SubtitlesHelperSrt.ConvertSubtitlesToText(subtitles);

        // Assert
        text.Should().NotContain("00:00");
        text.Should().NotContain("-->");
    }

    [TestMethod]
    public void When_SubtitlesArePreprocessed_Expect_TextIsPreserved()
    {
        // Arrange
        var subtitles = @"1
            00:00:00,000 --> 00:00:02,000
            Hello
            
            2
            00:00:02,000 --> 00:00:04,000
            World";

        // Act
        var text = SubtitlesHelperSrt.ConvertSubtitlesToText(subtitles);

        // Assert
        text.Should().Contain("Hello");
        text.Should().Contain("World");
    }

    [TestMethod]
    public void When_SubtitlesArePreprocessed_Expect_MultipleWhitespaceCharactersAreReducedToSingleSpace()
    {
        // Arrange
        var subtitles = @"1
            00:00:00,000 --> 00:00:02,000
            Hello
            
            2
            00:00:02,000 --> 00:00:04,000
            World";

        // Act
        var text = SubtitlesHelperSrt.ConvertSubtitlesToText(subtitles);

        // Assert
        text.Should().NotContain("  ");
        text.Should().NotContain("\t ");
        text.Should().NotContain("\t\t");
        text.Should().NotContain(" \r");
        text.Should().NotContain(" \n");
        text.Should().NotContain("\r ");
        text.Should().NotContain("\n ");
    }

    [TestMethod]
    public void When_RealWorldSubtitlesArePreprocessed_Expect_NoExceptions()
    {
        // Arrange
        var subtitles = File.ReadAllText("Resources/NightOfTheLivingDead.srt");

        // Act
        var text = SubtitlesHelperSrt.ConvertSubtitlesToText(subtitles);

        // Assert
        text.Should().NotStartWith("-->", because: "Timestamps should be all removed");
        text.Should().NotContain("\r\n\r\n", because: "There should be no empty lines in the text");
        text.Should().NotContain("\n\n", because: "There should be no empty lines in the text");
    }

}
