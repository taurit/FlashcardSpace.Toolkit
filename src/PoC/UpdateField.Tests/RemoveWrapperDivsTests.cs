using AnkiCardValidator.ViewModels;
using UpdateField.Utilities;

namespace UpdateField.Tests;

[TestClass]
public class RemoveWrapperDivsTests
{
    [TestMethod]
    public void RemoveWrapperDivs_RemovesWrapperDivsWhenTheyHaveNoAttributes()
    {
        // Arrange
        var fields = AnkiNote.SerializeFields("<div><div>Front text</div></div>", "", "<div><DIV>Back text</DIV></div>", "", "<DiV><div>Image</div></DiV>", "<Div><dIV>Remarks</div></div>");
        var note = new AnkiNote(1, "OneDirection", "", fields);

        // Act
        MutationHelpers.RemoveWrapperDivs(note);

        // Assert
        Assert.AreEqual("Front text", note.FrontText);
        Assert.AreEqual("Back text", note.BackText);
        Assert.AreEqual("Image", note.Image);
        Assert.AreEqual("Remarks", note.Remarks);
    }

    [TestMethod]
    public void RemoveWrapperDivs_KeepsWrapperDivsWhenTheyHaveSomeAttributes()
    {
        // Arrange
        var fields = AnkiNote.SerializeFields(
            "<div id=\"test\">Front text</div>", "",
            "<div class='test'>Back text</div>", "",
            "<div style=''>Image</div>",
            "<div isDisabled>Remarks</div>");
        var note = new AnkiNote(1, "OneDirection", "", fields);

        // Act
        MutationHelpers.RemoveWrapperDivs(note);

        // Assert
        Assert.AreEqual("<div id=\"test\">Front text</div>", note.FrontText);
        Assert.AreEqual("<div class='test'>Back text</div>", note.BackText);
        Assert.AreEqual("<div style=''>Image</div>", note.Image);
        Assert.AreEqual("<div isDisabled>Remarks</div>", note.Remarks);
    }

    [TestMethod]
    public void RemoveWrapperDivs_KeepsWrapperDivsIfTheyAreNotTheSingleRootElements()
    {
        // Arrange
        var fields = AnkiNote.SerializeFields(
            "<div></div><div>Front text</div>", "",
            "<b></b>Back <div>text</div>", "",
            "<div><div>Image</div> 1</div><span>AAA</span>",
            "Remarks");
        var note = new AnkiNote(1, "OneDirection", "", fields);

        // Act
        MutationHelpers.RemoveWrapperDivs(note);

        // Assert
        Assert.AreEqual("<div></div><div>Front text</div>", note.FrontText);
        Assert.AreEqual("<b></b>Back <div>text</div>", note.BackText);
        Assert.AreEqual("<div><div>Image</div> 1</div><span>AAA</span>", note.Image);
        Assert.AreEqual("Remarks", note.Remarks);
    }
}
