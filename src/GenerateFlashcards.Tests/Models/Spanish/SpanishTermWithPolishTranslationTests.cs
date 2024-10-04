using CoreLibrary;
using CoreLibrary.Services.ObjectGenerativeFill;
using FluentAssertions;
using GenerateFlashcards.Models.Spanish;
using GenerateFlashcards.Tests.TestInfrastructure;

namespace GenerateFlashcards.Tests.Models.Spanish;

[TestClass]
[TestCategory("RequiresGenerativeAi")]
public class SpanishTermWithPolishTranslationTests
{
    private readonly GenerativeFill _generativeFill = GenerativeFillTestFactory.CreateInstance();

    [DataTestMethod]
    [DataRow("grande", "duży")]
    [DataRow("pequeño", "mały")]
    [DataRow("rojo", "czerwony")]
    [DataRow("azul", "niebieski")]
    // below, more rare ones (C1)
    [DataRow("perezoso", "leniwy")]
    [DataRow("valiente", "odważny")]
    [DataRow("sabroso", "smaczny")]
    [DataRow("soso", "mdły")]
    public async Task SpanishAdjective_GetsCorrectlyTranslatedToPolish(string spanishAdjective, string expectedPolishEquivalent)
    {
        // Arrange
        var input = new SpanishTermWithPolishTranslation()
        {
            SpanishWord = spanishAdjective,
            SpanishWordPartOfSpeech = PartOfSpeech.Adjective,
            SpanishSentence = "Este es muy " + spanishAdjective + ".",
        };

        // Act
        var output = await _generativeFill.FillMissingProperties(
            TestParameters.OpenAiModelId, TestParameters.OpenAiModelId, input);

        // Assert
        output.SpanishWordEquivalentInPolish.Should().Be(expectedPolishEquivalent);
        output.SpanishSentenceEquivalentInPolish.Should().NotBeNullOrWhiteSpace();
    }
}
