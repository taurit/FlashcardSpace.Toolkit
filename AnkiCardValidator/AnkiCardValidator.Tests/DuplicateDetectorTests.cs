using AnkiCardValidator.Models;
using AnkiCardValidator.Utilities;
using AnkiCardValidator.ViewModels;
using FluentAssertions;

namespace AnkiCardValidator.Tests;

[TestClass]
public class DuplicateDetectorTests
{
    [TestMethod]
    public void WhenNoSideIsDuplicated_ExpectEmptyResult()
    {
        // Arrange
        var note1 = new AnkiNote(0, "OneDirection", " tagA ", "front1", "back1", "", "", "", "");
        var note2 = new AnkiNote(1, "OneDirection", " tagA ", "front2", "back2", "", "", "", "");
        var note3 = new AnkiNote(1, "OneDirection", " tagB ", "front3", "back3", "", "", "", "");

        var card1 = new CardViewModel(note1, false, FlashcardDirection.FrontTextInPolish, 0, 0, 0, 0, CefrClassification.A1, null, null);
        var card2 = new CardViewModel(note2, false, FlashcardDirection.FrontTextInPolish, 0, 0, 0, 0, CefrClassification.A1, null, null);
        var card3 = new CardViewModel(note3, false, FlashcardDirection.FrontTextInPolish, 0, 0, 0, 0, CefrClassification.A1, null, null);

        var ankiCards = new List<CardViewModel>() { card1, card2, card3, };
        var sut = new DuplicateDetector(new NormalFormProvider());

        // Act
        var resultFront1 = sut.DetectDuplicatesInQuestion(card1, ankiCards);
        var resultFront2 = sut.DetectDuplicatesInQuestion(card2, ankiCards);
        var resultFront3 = sut.DetectDuplicatesInQuestion(card3, ankiCards);

        // Assert
        resultFront1.Should().BeEmpty();
        resultFront2.Should().BeEmpty();
        resultFront3.Should().BeEmpty();
    }

    [DataTestMethod]
    [DataRow("test", "test")]
    [DataRow("test", "test *")] // typical for my imported Spanish deck
    [DataRow("test", "test * ")] // typical for my imported Spanish deck
    [DataRow("test", "Test")] // hypothetical: only differ in casing
    [DataRow("el pozo", "pozo")] // spanish: with & without article
    [DataRow("los pozos", "pozos")] // spanish: with & without article
    [DataRow("la muñeca", "muñeca")] // spanish: with & without article
    [DataRow("las muñecas", "muñecas")] // spanish: with & without article
    [DataRow("por un lado", "por un lado...")] // spanish: with & without ellipsis
    [DataRow("¡Hola!", "hola")] // spanish: with & without punctuation
    [DataRow("¿Cómo?", "cómo")] // spanish: with & without punctuation
    [DataRow("przed (domem)", "przed")]
    [DataRow("ładny (<i>ang. pretty</i>)", "ładny")]
    [DataRow("wiadomość, news", "wiadomość")] // todo similar tests could be useful in deduplication!
    [DataRow("(jaka) szkoda", "szkoda")]
    [DataRow("depozyt, kaucja (np. na czynsz)", "depozyt")]
    [DataRow("tym... (tym lepiej)", "tym")]
    [DataRow("1) człowiek<br />2) mężczyzna", "człowiek")]
    [DataRow("droga<br />(szlak, metaforyczne pojęcie)", "droga")]
    [DataRow("statek *", "statek")]
    public void SimilarEnough_AreDuplicates(string s1, string s2)
    {
        // Arrange
        var note1 = new AnkiNote(0, "OneDirection", " tag1 ", s1, "back1", "", "", "", "");
        var note2 = new AnkiNote(1, "OneDirection", " tag1 ", s2, "back2", "", "", "", "");

        var card1 = new CardViewModel(note1, false, FlashcardDirection.FrontTextInPolish, 0, 0, 0, 0, CefrClassification.A1, null, null);
        var card2 = new CardViewModel(note2, false, FlashcardDirection.FrontTextInPolish, 0, 0, 0, 0, CefrClassification.A1, null, null);

        var cards = new List<CardViewModel>() { card1, card2, };

        var sut = new DuplicateDetector(new NormalFormProvider());

        // Act
        var resultFront1 = sut.DetectDuplicatesInQuestion(card1, cards);
        var resultFront2 = sut.DetectDuplicatesInQuestion(card2, cards);

        // Assert
        resultFront1.Should().NotBeNull();
        resultFront1.Should().HaveCount(1);
        resultFront1.Should().Contain(card2);

        resultFront2.Should().NotBeNull();
        resultFront2.Should().HaveCount(1);
        resultFront2.Should().Contain(card1);
    }

    [TestMethod]
    public void DuplicatesAreDetectedInBothDirectionsCardsToo()
    {
        // Arrange
        var note1 = new AnkiNote(0, "OneDirection", " tag1 ", "kot", "el gato", "", "", "", "");
        var note2 = new AnkiNote(1, "BothDirections", " tag1 ", "el gato *", "kot *", "", "", "", "");

        var card1 = new CardViewModel(note1, false, FlashcardDirection.FrontTextInPolish, 0, 0, 0, 0, CefrClassification.A1, null, null);

        var card2A = new CardViewModel(note2, false, FlashcardDirection.FrontTextInSpanish, 0, 0, 0, 0, CefrClassification.A1, null, null);
        var card2B = new CardViewModel(note2, true, FlashcardDirection.FrontTextInSpanish, 0, 0, 0, 0, CefrClassification.A1, null, null);

        var cards = new List<CardViewModel>() { card1, card2A, card2B };

        var sut = new DuplicateDetector(new NormalFormProvider());

        // Act
        var foundDuplicates1 = sut.DetectDuplicatesInQuestion(card1, cards);
        var foundDuplicates2A = sut.DetectDuplicatesInQuestion(card2A, cards);
        var foundDuplicates2B = sut.DetectDuplicatesInQuestion(card2B, cards);

        // Assert
        foundDuplicates1.Should().NotBeNull();
        foundDuplicates1.Should().HaveCount(1);
        foundDuplicates1.Should().Contain(card2B);
        foundDuplicates1.Should().NotContain(card2A);

        foundDuplicates2A.Should().NotBeNull();
        foundDuplicates2A.Should().HaveCount(0);

        foundDuplicates2B.Should().NotBeNull();
        foundDuplicates2B.Should().HaveCount(1);
        foundDuplicates2B.Should().Contain(card1);
        foundDuplicates2B.Should().NotContain(card2A);
    }

    [TestMethod]
    public void WhenContentDiffersOnlyWithArticle_ExpectNotMarkedAsDuplicate()
    {
        // Arrange
        var note1 = new AnkiNote(0, "OneDirection", " tagA ", "el artista", "back1", "", "", "", "");
        var note2 = new AnkiNote(1, "OneDirection", " tagA ", "la artista", "back2", "", "", "", "");

        var card1 = new CardViewModel(note1, false, FlashcardDirection.FrontTextInPolish, 0, 0, 0, 0, CefrClassification.A1, null, null);
        var card2 = new CardViewModel(note2, false, FlashcardDirection.FrontTextInPolish, 0, 0, 0, 0, CefrClassification.A1, null, null);

        var ankiCards = new List<CardViewModel>() { card1, card2 };
        var sut = new DuplicateDetector(new NormalFormProvider());

        // Act
        var resultFront1 = sut.DetectDuplicatesInQuestion(card1, ankiCards);
        var resultFront2 = sut.DetectDuplicatesInQuestion(card2, ankiCards);

        // Assert
        resultFront1.Should().BeEmpty();
        resultFront2.Should().BeEmpty();
    }
}
