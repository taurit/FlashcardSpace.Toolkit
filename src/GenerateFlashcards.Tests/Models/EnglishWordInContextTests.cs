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
    public async Task WordCatShouldBeRecognizedAndClassifiedAsNoun()
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

    [TestMethod]
    public async Task WordRunShouldBeRecognizedAndClassifiedAsVerb()
    {
        // Arrange
        var input = new EnglishWordInContext() { Word = "run" };

        // Act
        var output = await _generativeFill.FillMissingProperties(TestParameters.OpenAiModelId, TestParameters.OpenAiModelId, input);

        // Assert
        output.Word.Should().Be("run");
        output.WordBaseForm.Should().Be("to run");
        output.SentenceExample.Should().NotBeNullOrEmpty();
        output.PartOfSpeech.Should().Be(DetectedPartOfSpeech.Verb);
    }

    [TestMethod]
    public async Task WordBlueShouldBeRecognizedAndClassifiedAsAdjective()
    {
        // Arrange
        var input = new EnglishWordInContext() { Word = "blue" };

        // Act
        var output = await _generativeFill.FillMissingProperties(TestParameters.OpenAiModelId, TestParameters.OpenAiModelId, input);

        // Assert
        output.Word.Should().Be("blue");
        output.WordBaseForm.Should().Be("blue");
        output.SentenceExample.Should().NotBeNullOrEmpty();
        output.PartOfSpeech.Should().Be(DetectedPartOfSpeech.Adjective);
    }

}
