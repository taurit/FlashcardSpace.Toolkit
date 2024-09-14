using CoreLibrary.Services.ObjectGenerativeFill;
using FluentAssertions;
using GenerateFlashcards.Models.Spanish;
using GenerateFlashcards.Tests.Infrastructure;
using GenerateFlashcards.Tests.TestInfrastructure;

namespace GenerateFlashcards.Tests.Models.Spanish;

[TestClass]
[TestCategory("RequiresGenerativeAi")]
public class SpanishWordPartsOfSpeechTests
{
    private readonly GenerativeFill _generativeFill = GenerativeFillTestFactory.CreateInstance();

    [TestMethod]
    public async Task WordJovenShouldBeRecognizedAndClassifiedAsAdjective()
    {
        // Arrange
        var input = new SpanishWordPartsOfSpeech() { IsolatedWord = "joven" };

        // Act
        var output = await _generativeFill.FillMissingProperties(TestParameters.OpenAiModelId, TestParameters.OpenAiModelId, input);

        // Assert
        output.Dump();
        output.IsolatedWord.Should().Be("joven");
        output.PossiblePartsOfSpeechUsage.Should().HaveCountGreaterOrEqualTo(1, because: "joven can serve as an adjective (joven) and eventually as a noun (literary) and nothing else.");
        output.PossiblePartsOfSpeechUsage.Should().ContainSingle(role => role.PartOfSpeech == SpanishPartOfSpeech.Adjetivo);

        var adjective = output.PossiblePartsOfSpeechUsage.First(role => role.PartOfSpeech == SpanishPartOfSpeech.Adjetivo);
        adjective.WordBaseForm.Should().Be("joven");
        adjective.SentenceExample.Should().NotBeNullOrEmpty();
        adjective.SentenceExample.Should().Contain("joven");

        var noun = output.PossiblePartsOfSpeechUsage.First(role => role.PartOfSpeech == SpanishPartOfSpeech.Sustantivo);
        noun.WordBaseForm.Should().Be("el joven");
        noun.SentenceExample.Should().NotBeNullOrEmpty();
        noun.SentenceExample.Should().Contain("joven");

    }

    [TestMethod]
    public async Task ArrayOfInputsShouldBeProcessed()
    {
        // Arrange
        var inputWord1 = new SpanishWordPartsOfSpeech() { IsolatedWord = "joven" };
        var inputWord2 = new SpanishWordPartsOfSpeech() { IsolatedWord = "jugando" };
        var inputWord3 = new SpanishWordPartsOfSpeech() { IsolatedWord = "Fríos" };
        var inputWord4 = new SpanishWordPartsOfSpeech() { IsolatedWord = "xyzxyzxyzxyzxyz" };
        var inputWords = new List<SpanishWordPartsOfSpeech>() { inputWord1, inputWord2, inputWord3, inputWord4 };

        // Act
        var output = await _generativeFill.FillMissingProperties(TestParameters.OpenAiModelId, TestParameters.OpenAiModelId, inputWords);

        // Assert
        output.Dump();

        output[0].IsolatedWord.Should().Be("joven");
        output[0].PossiblePartsOfSpeechUsage.Should().HaveCountGreaterOrEqualTo(1, because: "joven can serve as a noun (el joven) and an adjective (joven) and nothing else.");

        output[1].IsolatedWord.Should().Be("jugando");
        output[1].PossiblePartsOfSpeechUsage.Should().HaveCount(1, because: "jugando can serve as a verb (jugar) and nothing else.");
        output[1].PossiblePartsOfSpeechUsage[0].WordBaseForm.Should().Be("jugar");

        output[2].IsolatedWord.Should().Be("Fríos");
        output[2].PossiblePartsOfSpeechUsage.Should().HaveCountGreaterOrEqualTo(1, because: "Fríos can serve as an adjective (frío) or a noun");
        output[2].PossiblePartsOfSpeechUsage.Should().ContainSingle(role => role.PartOfSpeech == SpanishPartOfSpeech.Adjetivo);
        output[2].PossiblePartsOfSpeechUsage.First(role => role.PartOfSpeech == SpanishPartOfSpeech.Adjetivo).WordBaseForm.Should().Be("frío");
        // 'Fríos' as noun in plural form is rarely encountered, so some models return it, some don't ; I don't consider it error

        output[3].IsolatedWord.Should().Be("xyzxyzxyzxyzxyz");
        output[3].PossiblePartsOfSpeechUsage.Should().BeEmpty();
    }


    [TestMethod]
    public async Task Top10FrequentWordsAreProcessedProperly()
    {
        // Arrange
        var inputWord1 = new SpanishWordPartsOfSpeech() { IsolatedWord = "de" };
        var inputWord2 = new SpanishWordPartsOfSpeech() { IsolatedWord = "la" };
        var inputWord3 = new SpanishWordPartsOfSpeech() { IsolatedWord = "que" };
        var inputWord4 = new SpanishWordPartsOfSpeech() { IsolatedWord = "el" };
        var inputWord5 = new SpanishWordPartsOfSpeech() { IsolatedWord = "en" };
        var inputWord6 = new SpanishWordPartsOfSpeech() { IsolatedWord = "y" };
        var inputWord7 = new SpanishWordPartsOfSpeech() { IsolatedWord = "a" };
        var inputWord8 = new SpanishWordPartsOfSpeech() { IsolatedWord = "los" };
        var inputWord9 = new SpanishWordPartsOfSpeech() { IsolatedWord = "se" };
        var inputWord10 = new SpanishWordPartsOfSpeech() { IsolatedWord = "del" };
        var inputWords = new List<SpanishWordPartsOfSpeech>() {
            inputWord1, inputWord2, inputWord3, inputWord4, inputWord5, inputWord6,
            inputWord7, inputWord8, inputWord9, inputWord10
        };

        // Act
        var output = await _generativeFill.FillMissingProperties(TestParameters.OpenAiModelId, TestParameters.OpenAiModelId, inputWords);

        // Assert
        output.Dump();

        output.Should().HaveCount(10);

        output[0].IsolatedWord.Should().Be("de");
        output[0].PossiblePartsOfSpeechUsage.Should().NotContain(role => role.PartOfSpeech == SpanishPartOfSpeech.Adjetivo);
        output[0].PossiblePartsOfSpeechUsage.Should().NotContain(role => role.PartOfSpeech == SpanishPartOfSpeech.Verbo);
        output[0].PossiblePartsOfSpeechUsage.Should().NotContain(role => role.PartOfSpeech == SpanishPartOfSpeech.Sustantivo);

        output[1].IsolatedWord.Should().Be("la");
        output[1].PossiblePartsOfSpeechUsage.Should().HaveCountGreaterThanOrEqualTo(1, because: "`la` can serve as a definite article or a pronoun.");
        output[1].PossiblePartsOfSpeechUsage[0].PartOfSpeech.Should()
            .BeOneOf(SpanishPartOfSpeech.Determinante, SpanishPartOfSpeech.Pronombre);

        output[2].IsolatedWord.Should().Be("que");
        output[2].PossiblePartsOfSpeechUsage.Should().HaveCountGreaterOrEqualTo(1, because: "que can serve as a conjunction or pronoun.");
        // depending on context it can be a pronoun or conjunction, and ChatGPT gives different answers sometimes. So:
        output[2].PossiblePartsOfSpeechUsage[0].PartOfSpeech.Should().NotBe(SpanishPartOfSpeech.Verbo);
        output[2].PossiblePartsOfSpeechUsage[0].PartOfSpeech.Should().NotBe(SpanishPartOfSpeech.Sustantivo);
        output[2].PossiblePartsOfSpeechUsage[0].PartOfSpeech.Should().NotBe(SpanishPartOfSpeech.Adjetivo);

        output[3].IsolatedWord.Should().Be("el");
        output[3].PossiblePartsOfSpeechUsage.Should().HaveCount(1, because: "el can serve as a definite article and nothing else.");
        output[3].PossiblePartsOfSpeechUsage[0].PartOfSpeech.Should().Be(SpanishPartOfSpeech.Determinante);

        output[4].IsolatedWord.Should().Be("en");
        output[4].PossiblePartsOfSpeechUsage.Should().HaveCount(1, because: "en can serve as a preposition and nothing else.");
        output[4].PossiblePartsOfSpeechUsage[0].PartOfSpeech.Should().Be(SpanishPartOfSpeech.Preposicion);

        output[5].IsolatedWord.Should().Be("y");
        output[5].PossiblePartsOfSpeechUsage.Should().HaveCount(1, because: "y can serve as a conjunction and nothing else.");
        output[5].PossiblePartsOfSpeechUsage[0].PartOfSpeech.Should().Be(SpanishPartOfSpeech.Conjuncion);

        output[6].IsolatedWord.Should().Be("a");
        output[6].PossiblePartsOfSpeechUsage.Should().HaveCount(1, because: "a can serve as a preposition and nothing else.");
        output[6].PossiblePartsOfSpeechUsage[0].PartOfSpeech.Should().Be(SpanishPartOfSpeech.Preposicion);

        output[7].IsolatedWord.Should().Be("los");
        output[7].PossiblePartsOfSpeechUsage.Should().HaveCount(1, because: "los can serve as a definite article and nothing else.");
        output[7].PossiblePartsOfSpeechUsage[0].PartOfSpeech.Should().Be(SpanishPartOfSpeech.Determinante);

        output[8].IsolatedWord.Should().Be("se");
        output[8].PossiblePartsOfSpeechUsage.Should().HaveCount(1, because: "se can serve as a pronoun and nothing else."); // unlike sé
        output[8].PossiblePartsOfSpeechUsage[0].PartOfSpeech.Should().Be(SpanishPartOfSpeech.Pronombre);

        output[9].IsolatedWord.Should().Be("del");
        output[9].PossiblePartsOfSpeechUsage.Should().HaveCount(1, because: "del can serve as a contraction and nothing else.");
        // hard to say what it is, but it's at least clear what it's not:
        output[9].PossiblePartsOfSpeechUsage[0].PartOfSpeech.Should().NotBe(SpanishPartOfSpeech.Adjetivo);
        output[9].PossiblePartsOfSpeechUsage[0].PartOfSpeech.Should().NotBe(SpanishPartOfSpeech.Verbo);
        output[9].PossiblePartsOfSpeechUsage[0].PartOfSpeech.Should().NotBe(SpanishPartOfSpeech.Sustantivo);

    }

    [TestMethod]
    public async Task WordMonoShouldBeRecognizedAsBothAdjectiveAndNoun()
    {
        // Arrange
        var input = new SpanishWordPartsOfSpeech() { IsolatedWord = "mono" };

        // Act
        var output = await _generativeFill.FillMissingProperties(TestParameters.OpenAiModelId, TestParameters.OpenAiModelId, input);

        // Assert
        output.Dump();
        output.IsolatedWord.Should().Be("mono");
        output.PossiblePartsOfSpeechUsage.Should().HaveCount(2, because: "mono can serve as a noun (el mono) and an adjective (mono) and nothing else.");
        output.PossiblePartsOfSpeechUsage.Should().ContainSingle(role => role.PartOfSpeech == SpanishPartOfSpeech.Adjetivo);
        output.PossiblePartsOfSpeechUsage.Should().ContainSingle(role => role.PartOfSpeech == SpanishPartOfSpeech.Sustantivo);

        var adjective = output.PossiblePartsOfSpeechUsage.First(role => role.PartOfSpeech == SpanishPartOfSpeech.Adjetivo);
        adjective.WordBaseForm.Should().Be("mono");
        adjective.SentenceExample.Should().NotBeNullOrEmpty();
        adjective.SentenceExample.Should().ContainAny("mono", "mona");

        var noun = output.PossiblePartsOfSpeechUsage.First(role => role.PartOfSpeech == SpanishPartOfSpeech.Sustantivo);
        noun.WordBaseForm.Should().Be("el mono");
        noun.SentenceExample.Should().NotBeNullOrEmpty();
        noun.SentenceExample.Should().Contain("mono");
    }

}
