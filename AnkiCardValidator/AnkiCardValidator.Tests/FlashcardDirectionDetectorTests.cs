using AnkiCardValidator.Models;
using AnkiCardValidator.Utilities;
using FluentAssertions;

namespace AnkiCardValidator.Tests;

[TestClass]
public class FlashcardDirectionDetectorTests
{
    private static FlashcardDirectionDetector _sut = null!;

    [ClassInitialize]
    public static void ClassInitialize(TestContext context)
    {
        _sut = GetSut();
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
    [DataRow("Uruchom Todoist zminimalizowany", "Empezar Todoist minimizado", FlashcardDirection.QuestionInPolish)]
    [DataRow("sin embargo", "", FlashcardDirection.QuestionInSpanish)]
    [DataRow("bohater", "", FlashcardDirection.QuestionInPolish)]
    [DataRow("abrir", "", FlashcardDirection.QuestionInSpanish)]
    [DataRow("otwierać", "", FlashcardDirection.QuestionInPolish)]
    [DataRow("", "sin embargo", FlashcardDirection.QuestionInPolish)]
    [DataRow("", "otwierać", FlashcardDirection.QuestionInSpanish)]
    [DataRow("ahora", "teraz", FlashcardDirection.QuestionInSpanish)]
    [DataRow("nada", "nic", FlashcardDirection.QuestionInSpanish)]
    [DataRow("Cuba", "Kuba", FlashcardDirection.QuestionInSpanish)]
    [DataRow("tres", "trzy", FlashcardDirection.QuestionInSpanish)]
    [DataRow("torcer", "skręcać", FlashcardDirection.QuestionInSpanish)]
    [DataRow("yo", "ja", FlashcardDirection.QuestionInSpanish)]
    [DataRow("cero", "zero", FlashcardDirection.QuestionInSpanish)]
    [DataRow("el&nbsp;lavavajillas", "<div> </div> <div>&nbsp;zmywarka</div>", FlashcardDirection.QuestionInSpanish)]
    public void DetectDirectionOfACard(string frontSide, string backSide, FlashcardDirection expectedDirection)
    {
        // Arrange
        var note = new AnkiNote(0, frontSide, backSide, "", "OneDirection");

        // Act
        var direction = _sut.DetectDirectionOfACard(note);

        // Assert
        direction.Should().Be(expectedDirection);
    }

    private static FlashcardDirectionDetector GetSut()
    {
        var normalFormProvider = new NormalFormProvider();
        var polishFrequencyDataProvider = new FrequencyDataProvider(normalFormProvider, Settings.FrequencyDictionaryPolish);
        var spanishFrequencyDataProvider = new FrequencyDataProvider(normalFormProvider, Settings.FrequencyDictionarySpanish);
        var flashcardDirectionDetector = new FlashcardDirectionDetector(normalFormProvider, polishFrequencyDataProvider, spanishFrequencyDataProvider);
        return flashcardDirectionDetector;
    }
}
