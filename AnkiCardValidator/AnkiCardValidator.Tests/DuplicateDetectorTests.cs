using AnkiCardValidator.Utilities;
using FluentAssertions;

namespace AnkiCardValidator.Tests;

[TestClass]
public class DuplicateDetectorTests
{
    [TestMethod]
    public void WhenNoSideIsDuplicated_ExpectEmptyResult()
    {
        // Arrange
        var note1 = new AnkiNote(0, "front1", "back1", " tagA ");
        var note2 = new AnkiNote(1, "front2", "back2", " tagA ");
        var note3 = new AnkiNote(1, "front3", "back3", " tagB ");
        var ankiNotes = new List<AnkiNote>() { note1, note2, note3 };

        // Act
        var resultFront1 = DuplicateDetector.DetectDuplicatesFront(note1, ankiNotes);
        var resultFront2 = DuplicateDetector.DetectDuplicatesFront(note2, ankiNotes);
        var resultFront3 = DuplicateDetector.DetectDuplicatesFront(note3, ankiNotes);

        var resultBack1 = DuplicateDetector.DetectDuplicatesBack(note1, ankiNotes);
        var resultBack2 = DuplicateDetector.DetectDuplicatesBack(note2, ankiNotes);
        var resultBack3 = DuplicateDetector.DetectDuplicatesBack(note3, ankiNotes);

        // Assert
        resultFront1.Should().BeEmpty();
        resultFront2.Should().BeEmpty();
        resultFront3.Should().BeEmpty();

        resultBack1.Should().BeEmpty();
        resultBack2.Should().BeEmpty();
        resultBack3.Should().BeEmpty();
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
        var note1F = new AnkiNote(0, s1, "back1", " tag1 ");
        var note2F = new AnkiNote(1, s2, "back2", " tag1 ");
        var ankiNotesForTestingFront = new List<AnkiNote>() { note1F, note2F, };

        var note1B = new AnkiNote(0, "front1", s1, " tag1 ");
        var note2B = new AnkiNote(1, "front2", s2, " tag1 ");
        var ankiNotesForTestingBack = new List<AnkiNote>() { note1B, note2B, };

        // Act
        var resultFront1 = DuplicateDetector.DetectDuplicatesFront(note1F, ankiNotesForTestingFront);
        var resultFront2 = DuplicateDetector.DetectDuplicatesFront(note2F, ankiNotesForTestingFront);

        var resultBack1 = DuplicateDetector.DetectDuplicatesBack(note1B, ankiNotesForTestingBack);
        var resultBack2 = DuplicateDetector.DetectDuplicatesBack(note2B, ankiNotesForTestingBack);

        // Assert
        resultFront1.Should().NotBeNull();
        resultFront1.Should().HaveCount(1);
        resultFront1.Should().Contain(note2F);

        resultFront2.Should().NotBeNull();
        resultFront2.Should().HaveCount(1);
        resultFront2.Should().Contain(note1F);

        resultBack1.Should().NotBeNull();
        resultBack1.Should().HaveCount(1);
        resultBack1.Should().Contain(note2B);

        resultBack2.Should().NotBeNull();
        resultBack2.Should().HaveCount(1);
        resultBack2.Should().Contain(note1B);
    }
}
