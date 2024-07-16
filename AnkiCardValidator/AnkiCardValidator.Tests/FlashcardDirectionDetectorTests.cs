using AnkiCardValidator.Models;
using AnkiCardValidator.Utilities;
using FluentAssertions;

namespace AnkiCardValidator.Tests;

[TestClass]
public class FlashcardDirectionDetectorTests
{
    private static FlashcardDirectionDetector sut;

    [ClassInitialize]
    public static void ClassInitialize(TestContext context)
    {
        sut = GetSut();
    }

    [DataTestMethod]
    [DataRow("Hola, ¿cómo estás?", "Hej, jak się masz?", FlashcardDirection.QuestionInSpanish)]
    [DataRow("Hej, jak się masz?", "Hola, ¿cómo estás?", FlashcardDirection.QuestionInPolish)]
    [DataRow("abrir", "otwierać", FlashcardDirection.QuestionInSpanish)]
    [DataRow("adelante", "naprzód", FlashcardDirection.QuestionInSpanish)]
    [DataRow("alguno", "jakiś (mężczyzna)", FlashcardDirection.QuestionInSpanish)]
    [DataRow("bohater", "el héroe", FlashcardDirection.QuestionInPolish)]
    [DataRow("el caso", "przypadek", FlashcardDirection.QuestionInSpanish)]
    [DataRow("cuidadoso", "uważny", FlashcardDirection.QuestionInSpanish)]
    [DataRow("sin embargo", "pomimo to, niemniej jednak", FlashcardDirection.QuestionInSpanish)]
    public void DetectDirectionOfACard(string frontSide, string backSide, FlashcardDirection expectedDirection)
    {
        // Arrange
        var note = new AnkiNote(0, frontSide, backSide, "");

        // Act
        var direction = sut.DetectDirectionOfACard(note);

        // Assert
        direction.Should().Be(expectedDirection);
    }

    private static FlashcardDirectionDetector GetSut()
    {
        var normalFormProvider = new NormalFormProvider();
        var polishFrequencyDataProvider = new FrequencyDataProvider(normalFormProvider, Settings.FrequencyDictionaryPolish);
        var spanishFrequencyDataProvider = new FrequencyDataProvider(normalFormProvider, Settings.FrequencyDictionarySpanish);
        var flashcardDirectionDetector = new FlashcardDirectionDetector(polishFrequencyDataProvider, spanishFrequencyDataProvider);
        return flashcardDirectionDetector;
    }
}
