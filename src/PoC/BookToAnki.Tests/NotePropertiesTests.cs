using BookToAnki.Models;
using BookToAnki.NotePropertiesDatabase;
using BookToAnki.Services;
using FluentAssertions;
using System.Diagnostics;

namespace BookToAnki.Tests;

[TestClass]
public class NotePropertiesTests
{
    private NoteProperties GetSutWithNewTemporaryDatabase()
    {
        var databaseFileName = Path.GetTempFileName();
        if (File.Exists(databaseFileName))
            File.Delete(databaseFileName);
        var sut = new NoteProperties(databaseFileName, true);
        return sut;
    }

    [TestMethod]
    public void When_DatabaseFileDoesNotExist_ItIsCreated()
    {
        // Arrange
        var sut = GetSutWithNewTemporaryDatabase();
        var noteKey = new PrefKey("word", "sentence");

        // Act
        sut.SetNoteRating(noteKey, Rating.Premium);
        var noteRating = sut.GetNoteRating(noteKey);

        // Assert
        noteRating.Should().Be(Rating.Premium);
    }

    [TestMethod]
    public void When_PropertyIsSetTwice_ExpectSecondValueWins()
    {
        // Arrange
        var sut = GetSutWithNewTemporaryDatabase();
        var noteKey = new PrefKey("word", "sentence");

        // Act
        sut.SetNoteRating(noteKey, Rating.Premium);
        sut.SetNoteRating(noteKey, Rating.AcceptableForPragmatics);
        var noteRating = sut.GetNoteRating(noteKey);

        // Assert
        noteRating.Should().Be(Rating.AcceptableForPragmatics);
    }

    [TestMethod]
    public void When_EnumPropertyIsSetToNull_AlsoExpectNullWhenItIsRetrieved()
    {
        // Arrange
        var sut = GetSutWithNewTemporaryDatabase();
        var noteKey = new PrefKey("word", "sentence");

        // Act
        sut.SetNoteRating(noteKey, Rating.Premium);
        sut.SetNoteRating(noteKey, null);
        var noteRating = sut.GetNoteRating(noteKey);

        // Assert
        noteRating.Should().BeNull();
    }

    [TestMethod]
    public void When_StringPropertyIsSaved_ExpectValueCanBeRetrieved()
    {
        // Arrange
        var sut = GetSutWithNewTemporaryDatabase();
        var noteKey = new PrefKey("word", "sentence");

        // Act
        sut.SetWordNominativePl(noteKey, "yo dawg");
        var word = sut.GetWordNominativePl(noteKey);

        // Assert
        word.Should().Be("yo dawg");
    }

    [TestMethod]
    public void When_StringPropertyIsSetToNull_AlsoExpectNullWhenItIsRetrieved()
    {
        // Arrange
        var sut = GetSutWithNewTemporaryDatabase();
        var noteKey = new PrefKey("word", "sentence");

        // Act
        sut.SetWordNominativePl(noteKey, "yo dawg");
        sut.SetWordNominativePl(noteKey, null);
        var word = sut.GetWordNominativePl(noteKey);

        // Assert
        word.Should().BeNull();
    }

    [TestMethod]
    public void When_BothStringAndEnumAreSaved_ExpectBothCanBeRetrieved()
    {
        // Arrange
        var sut = GetSutWithNewTemporaryDatabase();
        var noteKey = new PrefKey("word", "sentence");

        // Act
        sut.SetWordNominativePl(noteKey, "hello, world");
        sut.SetNoteRating(noteKey, Rating.AcceptableForPragmatics);

        var rating = sut.GetNoteRating(noteKey);
        var word = sut.GetWordNominativePl(noteKey);

        // Assert
        rating.Should().Be(Rating.AcceptableForPragmatics);
        word.Should().Be("hello, world");
    }

    [TestMethod]
    public void When_AudioSampleIsSaved_ExpectItCanBeRetrieved()
    {
        // Arrange
        var sut = GetSutWithNewTemporaryDatabase();
        var noteKey = new PrefKey("word", "sentence");

        var objectToStore = new AudioSample("path/to_file.mp3", 1.0, 2);

        // Act
        sut.SetAudioSample(noteKey, objectToStore);
        var retrievedAudioSample = sut.GetAudioSample(noteKey);

        // Assert
        retrievedAudioSample.Should().NotBeNull();
        retrievedAudioSample!.PathToAudioFile.Should().Be("path/to_file.mp3");
        retrievedAudioSample!.FirstWordStartTime.Should().Be(1.0);
        retrievedAudioSample!.LastWordEndTime.Should().Be(2);
    }

    [TestMethod]
    public void When_WordRatingIsNotNull_ExpectItIsReturnedByGetAlreadyRated()
    {
        // Arrange
        var sut = GetSutWithNewTemporaryDatabase();
        sut.SetNoteRating(new PrefKey("word", "sentenceA"), Rating.Rejected);
        sut.SetNoteRating(new PrefKey("word", "sentenceB"), Rating.AcceptableForPragmatics);
        sut.SetNoteRating(new PrefKey("wordB", "sentenceB"), Rating.Premium);
        sut.SetNoteRating(new PrefKey("wordC", "wordWithNoRating"), null);

        // Act
        var alreadyRated = sut.GetAlreadyRated();

        // Assert
        alreadyRated.Should().NotBeNull();
        alreadyRated.Should().HaveCount(3);
        alreadyRated.Should().Contain(x => x.PrefKey.Word == "word" && x.PrefKey.SentenceExample == "sentenceA");
        alreadyRated.Should().Contain(x => x.PrefKey.Word == "word" && x.PrefKey.SentenceExample == "sentenceB");
        alreadyRated.Should().Contain(x => x.PrefKey.Word == "wordb" && x.PrefKey.SentenceExample == "sentenceB");
        alreadyRated.Should().NotContain(x => x.PrefKey.Word == "wordc" && x.PrefKey.SentenceExample == "wordWithNoRating");
    }

    [TestMethod]
    public void When_VariousOperationsAreMixes_ExpectNoEntityFrameworkExceptionAbutObjectAlreadyTracked()
    {
        // Arrange
        var sut = GetSutWithNewTemporaryDatabase();

        // Act, Assert
        sut.SetNoteRating(new PrefKey("word", "string"), Rating.Rejected);
        sut.SetAudioSample(new PrefKey("word", "aaa"), new AudioSample("path/to_file.mp3", 1.0, 2));
        sut.SetWordNominativePl(new PrefKey("word", "string"), "aaa");
        sut.GetAlreadyRated();
        sut.SetSentenceTranslationPl(new PrefKey("word", "string"), "bbb");
        sut.GetWordExplanationEn(new PrefKey("word", "*"));
        sut.GetAudioSample(new PrefKey("word", "aaa"));
    }

    [TestMethod]
    public void When_UserUsesWildcardForUnsupportedFields_ExpectInvalidOperationException()
    {
        // Arrange
        var sut = GetSutWithNewTemporaryDatabase();

        // Act, Assert
        Action setNoteRatingAction = () => sut.SetNoteRating(new PrefKey("word", "*"), Rating.Rejected);
        setNoteRatingAction.Should().Throw<InvalidOperationException>();

        Action setAudioSampleAction = () => sut.SetAudioSample(new PrefKey("word", "*"), new AudioSample("path/to_file.mp3", 1.0, 2));
        setAudioSampleAction.Should().Throw<InvalidOperationException>();

        Action setSentenceTranslationPlAction = () => sut.SetSentenceTranslationPl(new PrefKey("word", "*"), "bbb");
        setSentenceTranslationPlAction.Should().Throw<InvalidOperationException>();

        Action setSentenceTranslationEnAction = () => sut.SetSentenceTranslationEn(new PrefKey("word", "*"), "bbb");
        setSentenceTranslationEnAction.Should().Throw<InvalidOperationException>();
    }

    [TestMethod]
    public void When_UserUsesWildcardForSupportedFields_ExpectNoException()
    {
        // Arrange
        var sut = GetSutWithNewTemporaryDatabase();

        // Act, Assert
        sut.SetWordNominativeOriginal(new PrefKey("word", "*"), "aaa");
        sut.SetWordNominativePl(new PrefKey("word", "*"), "aaa");
        sut.SetWordNominativeEn(new PrefKey("word", "*"), "aaa");
        sut.SetWordExplanationPl(new PrefKey("word", "*"), "a");
        sut.SetWordExplanationEn(new PrefKey("word", "*"), "b");
    }

    // several rows to have more probability of catching numeric precision inaccuracy
    [DataTestMethod]
    [DataRow(5, 666)]
    [DataRow(0.999999999999, 999999999999)]
    [DataRow((double)1 / 3, 33333333333)]
    [DataRow(1, 1)]
    [DataRow(0.49, 49)]
    public void When_AudioShiftIsSerializedAndDeserialized_Expect_PreciselySameTimeSpanValues(double beginningShiftSeconds, long endShiftTicks)
    {
        // Arrange
        var timeShiftBeginning = TimeSpan.FromSeconds(beginningShiftSeconds);
        var timeShiftEnd = TimeSpan.FromTicks(endShiftTicks);
        var audioShift = new AudioShift(timeShiftBeginning, timeShiftEnd);

        // Arrange
        var sut = GetSutWithNewTemporaryDatabase();
        var noteKey = new PrefKey("word", "sentence");

        // Act
        sut.SetAudioSampleShift(noteKey, audioShift);
        var shiftReadBack = sut.GetAudioSampleShift(noteKey);

        // Assert
        shiftReadBack.Should().NotBeNull();
        shiftReadBack!.TimeShiftBeginning.Should().Be(timeShiftBeginning);
        shiftReadBack.TimeShiftEnd.Should().Be(timeShiftEnd);
    }

    [TestMethod]
    public void When_ChatGptExplanationIsSavedOnExampleLevel_ExpectValuesCanBeRetrieved()
    {
        // Arrange
        var sut = GetSutWithNewTemporaryDatabase();
        var noteKey = new PrefKey("word", "sentence");

        // Act, Assert
        sut.SetWordNominativeOriginalGpt4(noteKey, "nominativeGpt4");
        sut.GetWordNominativeOriginalGpt4(noteKey).Should().Be("nominativeGpt4");
        sut.GetWordNominativeOriginal(noteKey).Should().BeNull();

        sut.SetWordNominativePlGpt4(noteKey, "nominativeGpt4Pl");
        sut.GetWordNominativePlGpt4(noteKey).Should().Be("nominativeGpt4Pl");
        sut.GetWordNominativePl(noteKey).Should().BeNull();

        sut.SetWordNominativeEnGpt4(noteKey, "nominativeGpt4En");
        sut.GetWordNominativeEnGpt4(noteKey).Should().Be("nominativeGpt4En");
        sut.GetWordNominativeEn(noteKey).Should().BeNull();

        sut.SetWordExplanationPlGpt4(noteKey, "e1");
        sut.GetWordExplanationPlGpt4(noteKey).Should().Be("e1");
        sut.GetWordExplanationPl(noteKey).Should().BeNull();

        sut.SetWordExplanationEnGpt4(noteKey, "e2");
        sut.GetWordExplanationEnGpt4(noteKey).Should().Be("e2");
        sut.GetWordExplanationEn(noteKey).Should().BeNull();
    }

    [Ignore]
    [TestMethod]
    public void Migrate()
    {
        var databaseFileName = @"d:\Flashcards\Words\note_properties_database.sqlite";
        var dbContext = new NoteContext(databaseFileName);

        var notesToUpdate = dbContext.Notes.ToList();
        var allNotes = notesToUpdate.ToList();
        List<NotePropertiesEntity> newEntities = new List<NotePropertiesEntity>();

        // First pass: Collect entities to remove and prepare new entities
        foreach (var note in notesToUpdate)
        {
            var lowercasedWord = note.Word.ToLowerInvariant();
            if (note.Word != lowercasedWord)
            {
                // Prepare a new entity with the lowercased word and other properties
                var newEntity = new NotePropertiesEntity(lowercasedWord, note.Sentence, note.PropertyName)
                {
                    PropertyValue = note.PropertyValue
                };

                var conflictOld = allNotes.Where(x => x.Word == lowercasedWord && x.Sentence == note.Sentence && x.PropertyName == note.PropertyName).ToList();
                var conflictNew = newEntities.Where(x => x.Word == lowercasedWord && x.Sentence == note.Sentence && x.PropertyName == note.PropertyName).ToList();

                if (conflictOld.Count == 0 && conflictNew.Count == 0)
                    newEntities.Add(newEntity);
                else
                    Debug.WriteLine(conflictOld.Count + " " + conflictNew.Count);

                // Remove the old entity
                dbContext.Notes.Remove(note);
            }
        }

        // Save the changes after removal to clear the tracking
        dbContext.SaveChanges();

        // Second pass: Add new entities
        dbContext = new NoteContext(databaseFileName);
        dbContext.Notes.AddRange(newEntities);
        dbContext.SaveChanges();

        // TODO handle duplicated keys after lowercasing
    }

    // I didn't find any room for optimization while reading whole table
    [Ignore]
    [TestMethod]
    public void OptimizePerformance()
    {
        Stopwatch sw = Stopwatch.StartNew();
        var databaseFileName = @"d:\Flashcards\Words\note_properties_database.sqlite";
        var np = new NoteProperties(databaseFileName);
        sw.Stop();
        Console.WriteLine(sw.Elapsed.TotalMilliseconds);
    }
}
