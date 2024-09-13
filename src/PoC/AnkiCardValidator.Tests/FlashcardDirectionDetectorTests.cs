using AnkiCardValidator.Models;
using AnkiCardValidator.Utilities;
using AnkiCardValidator.ViewModels;
using CoreLibrary.Services;
using FluentAssertions;

namespace AnkiCardValidator.Tests;

[TestClass]
[TestCategory("SkipInGitHubActions")] // todo: use LFS to commit large dictionary and allow run in pipelines? disable? not sure yet
public class FlashcardDirectionDetectorTests
{
    private static FlashcardDirectionDetector _sut = null!;

    [ClassInitialize]
    public static void ClassInitialize(TestContext context)
    {
        _sut = GetSut();
    }

    [DataTestMethod]
    [DataRow("Hola, ¿cómo estás?", "Hej, jak się masz?", FlashcardDirection.FrontTextInSpanish)]
    [DataRow("Hej, jak się masz?", "Hola, ¿cómo estás?", FlashcardDirection.FrontTextInPolish)]
    [DataRow("abrir", "otwierać", FlashcardDirection.FrontTextInSpanish)]
    [DataRow("adelante", "naprzód", FlashcardDirection.FrontTextInSpanish)]
    [DataRow("alguno", "jakiś (mężczyzna)", FlashcardDirection.FrontTextInSpanish)]
    [DataRow("bohater", "el héroe", FlashcardDirection.FrontTextInPolish)]
    [DataRow("el caso", "przypadek", FlashcardDirection.FrontTextInSpanish)]
    [DataRow("cuidadoso", "uważny", FlashcardDirection.FrontTextInSpanish)]
    [DataRow("sin embargo", "pomimo to, niemniej jednak", FlashcardDirection.FrontTextInSpanish)]
    [DataRow("Uruchom Todoist zminimalizowany", "Empezar Todoist minimizado", FlashcardDirection.FrontTextInPolish)]
    [DataRow("sin embargo", "", FlashcardDirection.FrontTextInSpanish)]
    [DataRow("bohater", "", FlashcardDirection.FrontTextInPolish)]
    [DataRow("abrir", "", FlashcardDirection.FrontTextInSpanish)]
    [DataRow("otwierać", "", FlashcardDirection.FrontTextInPolish)]
    [DataRow("", "sin embargo", FlashcardDirection.FrontTextInPolish)]
    [DataRow("", "otwierać", FlashcardDirection.FrontTextInSpanish)]
    [DataRow("ahora", "teraz", FlashcardDirection.FrontTextInSpanish)]
    [DataRow("nada", "nic", FlashcardDirection.FrontTextInSpanish)]
    [DataRow("Cuba", "Kuba", FlashcardDirection.FrontTextInSpanish)]
    [DataRow("tres", "trzy", FlashcardDirection.FrontTextInSpanish)]
    [DataRow("torcer", "skręcać", FlashcardDirection.FrontTextInSpanish)]
    [DataRow("yo", "ja", FlashcardDirection.FrontTextInSpanish)]
    [DataRow("cero", "zero", FlashcardDirection.FrontTextInSpanish)]
    [DataRow("el&nbsp;lavavajillas", "<div> </div> <div>&nbsp;zmywarka</div>", FlashcardDirection.FrontTextInSpanish)]
    public void DetectDirectionOfACard(string frontSide, string backSide, FlashcardDirection expectedDirection)
    {
        // Arrange
        var fieldsRawOriginal = AnkiNote.SerializeFields(frontSide, "", backSide, "", "", "");
        var note = new AnkiNote(0, "OneDirection", "", fieldsRawOriginal);

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
