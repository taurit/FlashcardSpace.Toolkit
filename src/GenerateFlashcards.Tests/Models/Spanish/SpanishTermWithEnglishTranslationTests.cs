using CoreLibrary.Services.ObjectGenerativeFill;
using FluentAssertions;
using GenerateFlashcards.Models.Spanish;
using GenerateFlashcards.Tests.Infrastructure;

namespace GenerateFlashcards.Tests.Models.Spanish;

[TestClass]
[TestCategory("RequiresGenerativeAi")]
public class SpanishTermWithEnglishTranslationTests
{
    private readonly GenerativeFill _generativeFill = GenerativeFillTestFactory.CreateInstance();

    [DataTestMethod]
    [DataRow("grande", "big")]
    [DataRow("pequeño", "small")]
    [DataRow("rojo", "red")]
    [DataRow("azul", "blue")]
    // below, more rare ones (C1)
    [DataRow("perezoso", "lazy")]
    [DataRow("valiente", "brave")]
    [DataRow("sabroso", "tasty")]
    [DataRow("soso", "bland")]
    public async Task SpanishAdjective_GetsCorrectlyTranslatedToEnglish(string spanishAdjective, string expectedEnglishEquivalent)
    {
        // Arrange
        var input = new SpanishTermWithEnglishTranslation()
        {
            SpanishWord = spanishAdjective,
            SpanishSentence = "Este es muy " + spanishAdjective + ".",
        };

        // Act
        var output = await _generativeFill.FillMissingProperties(
            TestParameters.OpenAiModelId, TestParameters.OpenAiModelId, input);

        // Assert
        output.SpanishWordEquivalentInEnglish.Should().Be(expectedEnglishEquivalent);
        output.SpanishSentenceEquivalentInEnglish.Should().NotBeNullOrWhiteSpace();
    }
}
