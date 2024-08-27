using FluentAssertions;
using GenerateFlashcards.Services.SentenceExtractors;

namespace GenerateFlashcards.Tests.Services.SentenceExtractors;

[TestClass]
public class ReferenceSentenceExtractorTests
{
    [TestMethod]
    public async Task When_SingleSentenceInEnglishIsProcessed_Expect_OneSentenceInOutput()
    {
        // Arrange
        var sut = new ReferenceSentenceExtractor();
        var input = "This is a simple sentence.";
        var inputFileName = Path.GetTempFileName();
        await File.WriteAllTextAsync(inputFileName, input);

        // Act
        var result = await sut.ExtractSentences(inputFileName);

        // Assert
        // good sentence extractor should be able to keep punctuation, but here we just test a simple reference implementation
        result.Should().ContainSingle("This is a simple sentence");
    }

    [TestMethod]
    public async Task When_TwoSentencesInEnglishAreProcessed_Expect_BothAreFoundInOutput()
    {
        // Arrange
        var sut = new ReferenceSentenceExtractor();
        var input = "Is this the first sentence? No, it's the second!";
        var inputFileName = Path.GetTempFileName();
        await File.WriteAllTextAsync(inputFileName, input);

        // Act
        var result = await sut.ExtractSentences(inputFileName);

        // Assert
        // good sentence extractor should be able to keep punctuation, but here we just test a simple reference implementation
        result.Should().Contain("Is this the first sentence");
        result.Should().Contain("No, it's the second");
    }
}
