using FluentAssertions;
using SimpleWordExtractor;

namespace GenerateFlashcards.Tests.BuildingBlocks;

[TestClass]
public class SimpleWordExtractorTests
{
    [TestMethod]
    public async Task When_SimpleSentenceInEnglishIsProcessed_Expect_AllWordsAppearInOutput()
    {
        // Arrange
        var sut = new SimpleWordExtractor.SimpleWordExtractor();
        var input = "This is a simple sentence.";
        var inputFileName = Path.GetTempFileName();
        await File.WriteAllTextAsync(inputFileName, input);

        // Act
        var result = await sut.ExtractWords(inputFileName);

        // Assert
        Assert.IsNotNull(result);
        result.Should().HaveCount(5);
        result.Should().ContainSingle(word => word.Word == "This");
        result.Should().ContainSingle(word => word.Word == "is");
        result.Should().ContainSingle(word => word.Word == "a");
        result.Should().ContainSingle(word => word.Word == "simple");
        result.Should().ContainSingle(word => word.Word == "sentence");
    }

    [TestMethod]
    public async Task When_TwoSentencesInEnglishAreProcessed_Expect_AllWordsAppearInOutputWithCorrectParentSentenceLinked()
    {
        // Arrange
        var sut = new SimpleWordExtractor.SimpleWordExtractor();
        var input = "Is this the first sentence? No, it's the second!";
        var inputFileName = Path.GetTempFileName();
        await File.WriteAllTextAsync(inputFileName, input);

        // Act
        var result = await sut.ExtractWords(inputFileName);

        // Assert
        Assert.IsNotNull(result);
        result.Should().ContainSingle(word => word.Word == "first").Which.SentencesWhereWordIsFound.Should().ContainSingle("Is this a first sentence");
        result.Should().ContainSingle(word => word.Word == "second").Which.SentencesWhereWordIsFound.Should().ContainSingle("No, it's the second");
        result.Should().ContainSingle(word => word.Word == "the").Which.SentencesWhereWordIsFound.Should().HaveCount(2);
    }
}
