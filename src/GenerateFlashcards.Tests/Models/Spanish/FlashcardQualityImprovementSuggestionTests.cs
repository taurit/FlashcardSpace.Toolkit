using CoreLibrary.Services.ObjectGenerativeFill;
using FluentAssertions;
using GenerateFlashcards.Models.Spanish;
using GenerateFlashcards.Tests.Infrastructure;
using GenerateFlashcards.Tests.TestInfrastructure;

namespace GenerateFlashcards.Tests.Models.Spanish;

[TestClass]
[TestCategory("RequiresGenerativeAi")]
public class FlashcardQualityImprovementSuggestionTests
{
    private readonly GenerativeFill _generativeFill = GenerativeFillTestFactory.CreateInstance();

    [TestMethod]
    public async Task When_FlashcardIsSimpleAndCorrect_Expect_NoWarning()
    {
        // Arrange
        var flashcard = new FlashcardQualityImprovementSuggestion
        {
            FrontTextSpanish = "Hola",
            BackTextPolish = "Cześć",
            SentenceExampleSpanish = "Hola, ¿cómo estás?",
            SentenceExamplePolish = "Cześć, jak się masz?"
        };

        // Act
        var output = await _generativeFill.FillMissingProperties(TestParameters.OpenAiModelId, TestParameters.OpenAiModelId, flashcard);

        // Assert
        output.Dump();
        output.Suggestions.Should().Be("");
    }

    [TestMethod]
    public async Task When_FlashcardContainsErrorInDefiniteArticle_Expect_WarningIsPresent()
    {

        // Arrange
        var flashcard = new FlashcardQualityImprovementSuggestion
        {
            FrontTextSpanish = "el casa", // error in definite article! should be la casa.
            BackTextPolish = "dom",
            SentenceExampleSpanish = "La casa es bonita.",
            SentenceExamplePolish = "Dom jest ładny."
        };

        // Act
        var output = await _generativeFill.FillMissingProperties(TestParameters.OpenAiModelId, TestParameters.OpenAiModelId, flashcard);

        // Assert
        output.Dump();
        output.Suggestions.Should().NotBeEmpty();
        output.Suggestions.Should().Contain("casa");
    }

    [TestMethod]
    public async Task When_FlashcardContainsSubtleErrorLikeMissingNationalCharacters_Expect_WarningIsPresent()
    {

        // Arrange
        var flashcard = new FlashcardQualityImprovementSuggestion
        {
            FrontTextSpanish = "la casa",
            BackTextPolish = "dom",
            SentenceExampleSpanish = "La casa es bonita.",
            SentenceExamplePolish = "Dom jest ladny."
        };

        // Act
        var output = await _generativeFill.FillMissingProperties(TestParameters.OpenAiModelId, TestParameters.OpenAiModelId, flashcard);

        // Assert
        output.Dump();
        output.Suggestions.Should().NotBeEmpty();
        output.Suggestions.Should().Contain("ladny");
    }

    [TestMethod]
    public async Task When_TranslationIsIncorrect_Expect_WarningIsPresent()
    {
        // Arrange
        var flashcard = new FlashcardQualityImprovementSuggestion
        {
            FrontTextSpanish = "la casa",
            BackTextPolish = "lalka",
            SentenceExampleSpanish = "La casa es bonita.",
            SentenceExamplePolish = "Lalka jest ładna."
        };

        // Act
        var output = await _generativeFill.FillMissingProperties(TestParameters.OpenAiModelId, TestParameters.OpenAiModelId, flashcard);

        // Assert
        output.Dump();
        output.Suggestions.Should().NotBeEmpty();
        output.Suggestions.Should().Contain("dom");
    }


    [TestMethod]
    public async Task When_TranslationContainsTypoLikeError_Expect_WarningIsPresent()
    {
        // Arrange
        var flashcard = new FlashcardQualityImprovementSuggestion
        {
            FrontTextSpanish = "la vela",
            BackTextPolish = "świeca",
            SentenceExampleSpanish = "Encendimos la vela durante la cena.",
            SentenceExamplePolish = "Zapaliśmy świecę podczas kolacji." // contains error (zapaliśmy instead of zapaliliśmy)
        };

        // Act
        var output = await _generativeFill.FillMissingProperties(TestParameters.OpenAiModelId, TestParameters.OpenAiModelId, flashcard);

        // Assert
        output.Dump();
        output.Suggestions.Should().NotBeEmpty();
        output.Suggestions.Should().Contain("zapaliliśmy");
    }
}
