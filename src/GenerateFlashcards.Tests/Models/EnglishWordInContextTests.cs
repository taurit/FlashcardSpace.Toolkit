using CoreLibrary.Services.ObjectGenerativeFill;
using FluentAssertions;
using GenerateFlashcards.Models;
using GenerateFlashcards.Tests.Infrastructure;

namespace GenerateFlashcards.Tests.Models;

[TestClass, TestCategory("SkipInGitHubActions")]
//[Ignore("Skipped to avoid unnecessary costs. Uncomment when modifying the service or changing the AI model.")]
public class GenerativeFillTests
{
    private readonly GenerativeFill _generativeFill = GenerativeFillTestFactory.CreateInstance();

    [TestMethod]
    public async Task WordCatShouldBeRecognizedAndClassified()
    {
        // Arrange
        var input = new EnglishWordInContext() { Word = "cat" };

        // Act
        var output = await _generativeFill.FillMissingProperties(TestParameters.OpenAiModelId, TestParameters.OpenAiModelId, input);

        // Assert
        output.Word.Should().Be("cat");
        output.WordBaseForm.Should().Be("a cat");
        output.SentenceExample.Should().NotBeNullOrEmpty();
        output.PartOfSpeech.Should().Be(DetectedPartOfSpeech.Noun);
    }
}
