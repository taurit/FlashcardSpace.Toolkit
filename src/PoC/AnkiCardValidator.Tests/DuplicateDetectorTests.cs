using AnkiCardValidator.Models;
using AnkiCardValidator.Utilities;
using AnkiCardValidator.ViewModels;
using CoreLibrary.Services;
using FluentAssertions;
using System.Collections.ObjectModel;

namespace AnkiCardValidator.Tests;

[TestClass]
public class DuplicateDetectorTests
{
    [TestMethod]
    public void WhenNoSideIsDuplicated_ExpectEmptyResult()
    {
        // Arrange
        var note1 = new AnkiNote(0, "OneDirection", " tagA ", AnkiNote.SerializeFields("front1", "back1", "", "", "", ""));
        var note2 = new AnkiNote(1, "OneDirection", " tagA ", AnkiNote.SerializeFields("front2", "back2", "", "", "", ""));
        var note3 = new AnkiNote(1, "OneDirection", " tagB ", AnkiNote.SerializeFields("front3", "back3", "", "", "", ""));

        var card1 = new CardViewModel(note1, false, FlashcardDirection.FrontTextInPolish, 0, 0, 0, 0, CefrClassification.A1, null, null);
        var card2 = new CardViewModel(note2, false, FlashcardDirection.FrontTextInPolish, 0, 0, 0, 0, CefrClassification.A1, null, null);
        var card3 = new CardViewModel(note3, false, FlashcardDirection.FrontTextInPolish, 0, 0, 0, 0, CefrClassification.A1, null, null);

        var ankiCards = new ObservableCollection<CardViewModel> { card1, card2, card3, };
        var sut = new DuplicateDetector(new NormalFormProvider());

        // Act
        sut.DetectDuplicatesInQuestion(ankiCards);

        // Assert
        card1.DuplicatesOfQuestion.Should().BeEmpty();
        card2.DuplicatesOfQuestion.Should().BeEmpty();
        card3.DuplicatesOfQuestion.Should().BeEmpty();
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
        var note1 = new AnkiNote(0, "OneDirection", " tag1 ", AnkiNote.SerializeFields(s1, "back1", "", "", "", ""));
        var note2 = new AnkiNote(1, "OneDirection", " tag1 ", AnkiNote.SerializeFields(s2, "back2", "", "", "", ""));

        var card1 = new CardViewModel(note1, false, FlashcardDirection.FrontTextInPolish, 0, 0, 0, 0, CefrClassification.A1, null, null);
        var card2 = new CardViewModel(note2, false, FlashcardDirection.FrontTextInPolish, 0, 0, 0, 0, CefrClassification.A1, null, null);

        var cards = new ObservableCollection<CardViewModel> { card1, card2, };

        var sut = new DuplicateDetector(new NormalFormProvider());

        // Act
        sut.DetectDuplicatesInQuestion(cards);

        // Assert
        card1.DuplicatesOfQuestion.Should().NotBeNull();
        card1.DuplicatesOfQuestion.Should().HaveCount(1);
        card1.DuplicatesOfQuestion.Should().Contain(card2);

        card2.DuplicatesOfQuestion.Should().NotBeNull();
        card2.DuplicatesOfQuestion.Should().HaveCount(1);
        card2.DuplicatesOfQuestion.Should().Contain(card1);
    }

    [TestMethod]
    public void DuplicatesAreDetectedInBothDirectionsCardsToo()
    {
        // Arrange
        var note1 = new AnkiNote(0, "OneDirection", " tag1 ", AnkiNote.SerializeFields("kot", "", "el gato", "", "", ""));
        var note2 = new AnkiNote(1, "BothDirections", " tag1 ", AnkiNote.SerializeFields("el gato *", "", "kot *", "", "", ""));

        var card1 = new CardViewModel(note1, false, FlashcardDirection.FrontTextInPolish, 0, 0, 0, 0, CefrClassification.A1, null, null);

        var card2A = new CardViewModel(note2, false, FlashcardDirection.FrontTextInSpanish, 0, 0, 0, 0, CefrClassification.A1, null, null);
        var card2B = new CardViewModel(note2, true, FlashcardDirection.FrontTextInSpanish, 0, 0, 0, 0, CefrClassification.A1, null, null);

        var cards = new ObservableCollection<CardViewModel> { card1, card2A, card2B };

        var sut = new DuplicateDetector(new NormalFormProvider());

        // Act
        sut.DetectDuplicatesInQuestion(cards);

        // Assert
        card1.DuplicatesOfQuestion.Should().NotBeNull();
        card1.DuplicatesOfQuestion.Should().HaveCount(1);
        card1.DuplicatesOfQuestion.Should().Contain(card2B);
        card1.DuplicatesOfQuestion.Should().NotContain(card2A);

        card2A.DuplicatesOfQuestion.Should().NotBeNull();
        card2A.DuplicatesOfQuestion.Should().HaveCount(0);

        card2B.DuplicatesOfQuestion.Should().NotBeNull();
        card2B.DuplicatesOfQuestion.Should().HaveCount(1);
        card2B.DuplicatesOfQuestion.Should().Contain(card1);
        card2B.DuplicatesOfQuestion.Should().NotContain(card2A);
    }

    [TestMethod]
    public void WhenContentDiffersOnlyWithArticle_ExpectNotMarkedAsDuplicate()
    {
        // Arrange
        var note1 = new AnkiNote(0, "OneDirection", " tagA ", AnkiNote.SerializeFields("el artista", "back1", "", "", "", ""));
        var note2 = new AnkiNote(1, "OneDirection", " tagA ", AnkiNote.SerializeFields("la artista", "back2", "", "", "", ""));

        var card1 = new CardViewModel(note1, false, FlashcardDirection.FrontTextInPolish, 0, 0, 0, 0, CefrClassification.A1, null, null);
        var card2 = new CardViewModel(note2, false, FlashcardDirection.FrontTextInPolish, 0, 0, 0, 0, CefrClassification.A1, null, null);

        var ankiCards = new ObservableCollection<CardViewModel> { card1, card2 };
        var sut = new DuplicateDetector(new NormalFormProvider());

        // Act
        sut.DetectDuplicatesInQuestion(ankiCards);

        // Assert
        card1.DuplicatesOfQuestion.Should().BeEmpty();
        card2.DuplicatesOfQuestion.Should().BeEmpty();
    }
}
