using CoreLibrary.Services.ObjectGenerativeFill;
using FluentAssertions;
using GenerateFlashcards.Models.Spanish;
using GenerateFlashcards.Tests.Infrastructure;
using GenerateFlashcards.Tests.TestInfrastructure;

namespace GenerateFlashcards.Tests.Models.Spanish;

[TestClass]
[TestCategory("RequiresGenerativeAi")]
public class SpanishTermGenericDetectorTests
{
    private readonly GenerativeFill _generativeFill = GenerativeFillTestFactory.CreateInstance();

    [DataTestMethod]
    [DataRow("mono")]
    [DataRow("gato")]
    [DataRow("perro")]
    [DataRow("coche")]
    public async Task NounsInSpanish_ShouldBeNormalizedAndContainArticle(string term)
    {
        // Arrange
        var input = new SpanishTermGenericDetector() { TermToCreateFlashcardFor = term };

        // Act
        var output = await _generativeFill.FillMissingProperties(TestParameters.OpenAiModelId, TestParameters.OpenAiModelId, input);

        // Assert
        output.Dump();

        output.FlashcardQuestionInSpanish.Should().NotBeNullOrEmpty();
        output.FlashcardQuestionInSpanish.Should().Contain("el ");

        output.FlashcardAnswerInPolish.Should().NotBeNullOrEmpty();
        output.Explanation.Should().NotBeNullOrEmpty();
    }


    [DataTestMethod]
    [DataRow("Hay nubes y *claros*", "claro", "nube")]
    [DataRow("un *ramo* de flores", "ramo", "flores")]
    public async Task WhenInputContainsHighlightedWord_ExpectItToBeCenterOfFocus(string term, string expected, string notExpected)
    {
        // Arrange
        var input = new SpanishTermGenericDetector() { TermToCreateFlashcardFor = term };

        // Act
        var output = await _generativeFill.FillMissingProperties(TestParameters.OpenAiModelId, TestParameters.OpenAiModelId, input);

        // Assert
        output.Dump();

        output.FlashcardQuestionInSpanish.Should().NotBeNullOrEmpty();
        output.FlashcardQuestionInSpanish.Should().Contain(expected);
        output.FlashcardQuestionInSpanish.Should().NotContain(notExpected);

        output.FlashcardExampleSentenceInSpanish.Should().NotBeNullOrEmpty();
        output.FlashcardExampleSentenceInSpanish.Should().Contain(expected);
    }

    [TestMethod]
    public async Task WhenInputContainsWordInPolish_ExpectFlashcardIsStillFromSpanishToPolishButHintsAreTaken()
    {
        // Arrange
        var inputO = new SpanishTermGenericDetector() { TermToCreateFlashcardFor = "kasztan (owoc)" };
        var inputD = new SpanishTermGenericDetector() { TermToCreateFlashcardFor = "kasztan (drzewo)" };

        // Act
        var outputO = await _generativeFill.FillMissingProperties(TestParameters.OpenAiModelId, TestParameters.OpenAiModelId, inputO);
        var outputD = await _generativeFill.FillMissingProperties(TestParameters.OpenAiModelId, TestParameters.OpenAiModelId, inputD);

        // Assert
        outputO.Dump();
        outputD.Dump();

        outputO.FlashcardQuestionInSpanish.Should().NotBeNullOrEmpty();
        outputO.FlashcardQuestionInSpanish.Should().Contain("la castaña");
        outputO.FlashcardAnswerInPolish.Should().NotBeNullOrEmpty();
        outputO.FlashcardAnswerInPolish.Should().Contain("kasztan");

        outputD.FlashcardQuestionInSpanish.Should().NotBeNullOrEmpty();
        outputD.FlashcardQuestionInSpanish.Should().Contain("el castaño");
        outputD.FlashcardAnswerInPolish.Should().NotBeNullOrEmpty();
        outputD.FlashcardAnswerInPolish.Should().Contain("kasztan");

    }

    [TestMethod]
    public async Task WhenInputContainsMixedLanguageContent_ExpectCorrectInterpretationOfWhatIsTaught()
    {
        // Arrange
        var input = new SpanishTermGenericDetector() { TermToCreateFlashcardFor = "Qué les pongo? (co wam nałożyć?)" };

        // Act
        var output = await _generativeFill.FillMissingProperties(TestParameters.OpenAiModelId, TestParameters.OpenAiModelId, input);

        // Assert
        output.Dump();

        output.FlashcardQuestionInSpanish.Should().NotBeNullOrEmpty();
        output.FlashcardQuestionInSpanish.Should().Contain("Qué les pongo");
        output.FlashcardQuestionInSpanish.Should().NotContain("nałożyć");
    }

    [DataTestMethod]
    [DataRow("Yo canto *peor que* tú.", "peor que")]
    [DataRow("Ella es *más alta que* él.", "más alta que")]
    [DataRow("El coche es *más rápido que* la bicicleta.", "más rápido que")]
    public async Task WhenFocusIsOnMultiwordGrammarStructures_TheyShouldBePresentInQuestion(string hint, string expected)
    {
        // Arrange
        var input = new SpanishTermGenericDetector() { TermToCreateFlashcardFor = hint };

        // Act
        var output = await _generativeFill.FillMissingProperties(TestParameters.OpenAiModelId, TestParameters.OpenAiModelId, input);

        // Assert
        output.Dump();

        output.FlashcardQuestionInSpanish.Should().NotBeNullOrEmpty();
        output.FlashcardQuestionInSpanish.Should().Contain(expected);

    }

    [TestMethod]
    public async Task WhenInputContainsWordFromLatinAmerica_ExpectItIsMentionedInRemarks()
    {
        // Arrange
        var input = new SpanishTermGenericDetector() { TermToCreateFlashcardFor = "durazno" };

        // Act
        var output = await _generativeFill.FillMissingProperties(TestParameters.OpenAiModelId, TestParameters.OpenAiModelId, input);

        // Assert
        output.Dump();

        output.FlashcardQuestionInSpanish.Should().NotBeNullOrEmpty();
        output.FlashcardQuestionInSpanish.Should().Contain("el durazno");

        output.Remarks.Should().NotBeNullOrEmpty();
        output.Remarks.Should().Contain("Latin");
        output.Remarks.Should().Contain("melocotón");
    }



}
