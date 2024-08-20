using AnkiCardValidator.ViewModels;
using FluentAssertions;

namespace AnkiCardValidator.Tests;

[TestClass]
public class AnkiNoteTests
{
    [TestMethod]
    public void WhenRealWorldFieldsStringIsSerializedBack_ExpectIdenticalStrings()
    {
        // Arrange
        var fieldsRawOriginal = "ли́стя сала́ту<div>[sound:rec1545424074.mp3]</div>\u001f\u001f<img src=\"paste-50306951938049.webp\"><div>lettuce leaves</div>\u001f\u001f\u001f";
        var ankiNote = new AnkiNote(0, "OneDirection", "", fieldsRawOriginal);

        // Act, Assert
        ankiNote.FieldsRawCurrent.Should().Be(fieldsRawOriginal);
    }

    [TestMethod]
    public void WhenRealWorldFieldsStringIsModifiedAndSerializedBack_ExpectNonIdenticalStrings()
    {
        // Arrange
        var fieldsRawOriginal = "ли́стя сала́ту<div>[sound:rec1545424074.mp3]</div>\u001f\u001f<img src=\"paste-50306951938049.webp\"><div>lettuce leaves</div>\u001f\u001f\u001f";
        var ankiNote = new AnkiNote(0, "OneDirection", "", fieldsRawOriginal);

        ankiNote.FrontText = "new front text";

        // Act, Assert
        ankiNote.FieldsRawCurrent.Should().NotBe(fieldsRawOriginal);
    }

    [TestMethod]
    public void WhenRealWorldFieldsStringIsModifiedAndSerializedBack_ExpectItCanBeParsedAgainAndContainsProperValues()
    {
        // Arrange
        var fieldsRawOriginal = "frontText\u001ffrontAudio\u001fbackText\u001fbackAudio\u001fimage\u001fcomments";
        var ankiNote = new AnkiNote(0, "OneDirection", "", fieldsRawOriginal);

        ankiNote.FieldsRawOriginal.Should().Be(fieldsRawOriginal);
        ankiNote.FieldsRawCurrent.Should().Be(fieldsRawOriginal);

        // Act
        ankiNote.FrontText = "new front text";
        ankiNote.FrontAudio = "new front audio";

        var updatedFieldValue = ankiNote.FieldsRawCurrent;
        var ankiNote2 = new AnkiNote(0, "OneDirection", "", updatedFieldValue);

        ankiNote2.FrontText.Should().Be("new front text");
        ankiNote2.FrontAudio.Should().Be("new front audio");
        ankiNote2.BackText.Should().Be("backText");
        ankiNote2.BackAudio.Should().Be("backAudio");
        ankiNote2.Image.Should().Be("image");
        ankiNote2.Remarks.Should().Be("comments");
    }
}
