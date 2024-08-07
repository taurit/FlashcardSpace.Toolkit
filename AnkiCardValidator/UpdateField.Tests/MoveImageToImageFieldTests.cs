using AnkiCardValidator.ViewModels;
using FluentAssertions;
using UpdateField.Mutations;

namespace UpdateField.Tests;

[TestClass]
public class MoveImageToImageFieldTests
{
    [TestMethod]
    public void MigrateImageToImageField_ShouldMoveImageToImageField_WhenImageInFrontText()
    {
        // Arrange
        var frontText = "<img src=\"paste-8731668512769_1526937749246.webp\"><div>to fear, to be afraid (of)</div>";
        var backText = "This is the back text";
        var fields = AnkiNote.SerializeFields(frontText, "", backText, "", "", "");
        var note = new AnkiNote(0, "template", "", fields);

        // Act
        MoveImageToImageField.MigrateImageToImageField(note);

        // Assert
        note.Image.Should().Be("<img src=\"paste-8731668512769_1526937749246.webp\">");
        note.FrontText.Should().Be("<div>to fear, to be afraid (of)</div>");
    }

    [TestMethod]
    public void MigrateImageToImageField_ShouldMoveImageToImageField_WhenImageInFrontTextIsNestedInDiv()
    {
        // Arrange
        var frontText = "<div><img src=\"paste-8731668512769_1526937749246.webp\"></div><div>to fear, to be afraid (of)</div>";
        var backText = "This is the back text";
        var fields = AnkiNote.SerializeFields(frontText, "", backText, "", "", "");
        var note = new AnkiNote(0, "template", "", fields);

        // Act
        MoveImageToImageField.MigrateImageToImageField(note);

        // Assert
        note.Image.Should().Be("<img src=\"paste-8731668512769_1526937749246.webp\">");
        note.FrontText.Should().Be("<div>to fear, to be afraid (of)</div>");
    }

    [TestMethod]
    public void MigrateImageToImageField_ShouldMoveImageToImageField_AndRemoveLeadingDivWithLineBreakInside()
    {
        // Arrange
        var frontText = "<div><img src=\"parcel-map.webp\"><br></div>to track a parcel";
        var backText = "This is the back text";
        var fields = AnkiNote.SerializeFields(frontText, "", backText, "", "", "");
        var note = new AnkiNote(0, "template", "", fields);

        // Act
        MoveImageToImageField.MigrateImageToImageField(note);

        // Assert
        note.Image.Should().Be("<img src=\"parcel-map.webp\">");
        note.FrontText.Should().Be("to track a parcel");
    }


    [TestMethod]
    public void MigrateImageToImageField_ShouldMoveImageToImageField_WhenImageInBackText()
    {
        // Arrange
        var frontText = "This is the front text";
        var backText = "<img src=\"paste-8731668512769_1526937749246.webp\"><div>to fear, to be afraid (of)</div>";
        var fields = AnkiNote.SerializeFields(frontText, "", backText, "", "", "");
        var note = new AnkiNote(0, "template", "", fields);

        // Act
        MoveImageToImageField.MigrateImageToImageField(note);

        // Assert
        note.Image.Should().Be("<img src=\"paste-8731668512769_1526937749246.webp\">");
        note.BackText.Should().Be("<div>to fear, to be afraid (of)</div>");
    }

    [TestMethod]
    public void MigrateImageToImageField_ShouldNotMoveImageToImageField_WhenNoImageInFields()
    {
        // Arrange
        var frontText = "This is the front text";
        var backText = "This is the back text";
        var fields = AnkiNote.SerializeFields(frontText, "", backText, "", "", "");
        var note = new AnkiNote(0, "template", "", fields);

        // Act
        MoveImageToImageField.MigrateImageToImageField(note);

        // Assert
        note.Image.Should().Be("");
        note.FrontText.Should().Be("This is the front text");
        note.BackText.Should().Be("This is the back text");
    }

    [TestMethod]
    public void MigrateImageToImageField_ShouldNotMoveImageToImageField_WhenBothFrontTextAndBackTextContainImage()
    {
        // Arrange
        var frontText = "<img src=\"a.jpg\">to fear";
        var backText = "<img src=\"b.jpg\">to fear";
        var fields = AnkiNote.SerializeFields(frontText, "", backText, "", "", "");
        var note = new AnkiNote(0, "template", "", fields);

        // Act
        MoveImageToImageField.MigrateImageToImageField(note);

        // Assert
        note.Image.Should().Be("");
        note.FrontText.Should().Be("<img src=\"a.jpg\">to fear");
        note.BackText.Should().Be("<img src=\"b.jpg\">to fear");
    }

    [TestMethod]
    public void MigrateImageToImageField_ShouldNotMoveImageToImageField_IfImageFieldAlreadyContainsSomeContent()
    {
        // Arrange
        var frontText = "<img src=\"a.jpg\">to fear";
        var image = "<img src=\"some-other-image.webp\">";
        var fields = AnkiNote.SerializeFields(frontText, "", "", "", image, "");
        var note = new AnkiNote(0, "template", "", fields);

        // Act
        MoveImageToImageField.MigrateImageToImageField(note);

        // Assert
        note.Image.Should().Be(image);
        note.FrontText.Should().Be("<img src=\"a.jpg\">to fear");
    }

    [DataTestMethod]
    [DataRow("<img src=\"a.jpg\"><br>to fear")]
    [DataRow("<img src=\"a.jpg\"><br/>to fear")]
    [DataRow("<img src=\"a.jpg\"><br />to fear")]
    [DataRow("<img src=\"a.jpg\"><br> to fear")]
    [DataRow("<img src=\"a.jpg\"><br/> to fear")]
    [DataRow("<img src=\"a.jpg\"><br /> to fear")]
    [DataRow("<img src=\"a.jpg\">\nto fear")]
    [DataRow("<img src=\"a.jpg\">\n<br />to fear")]
    [DataRow("<img src=\"a.jpg\">\n\tto fear")]
    public void MigrateImageToImageField_ShouldRemoveLineBreak_IfItFollowsMovedImage(string frontText)
    {
        // Arrange
        var fields = AnkiNote.SerializeFields(frontText, "", "", "", "", "");
        var note = new AnkiNote(0, "template", "", fields);

        // Act
        MoveImageToImageField.MigrateImageToImageField(note);

        // Assert
        note.Image.Should().Be("<img src=\"a.jpg\">");
        note.FrontText.Should().Be("to fear");
    }

    [TestMethod]
    public void MigrateImageToImageField_ShouldNotRemoveLineBreak_IfItIsBetweenTextFragments()
    {
        // Arrange
        var frontText = "<img src=\"a.jpg\"><br>to fear<br>to be afraid";
        var fields = AnkiNote.SerializeFields(frontText, "", "", "", "", "");
        var note = new AnkiNote(0, "template", "", fields);

        // Act
        MoveImageToImageField.MigrateImageToImageField(note);

        // Assert
        note.Image.Should().Be("<img src=\"a.jpg\">");
        note.FrontText.Should().Be("to fear<br>to be afraid");
    }

    [TestMethod]
    public void MigrateImageToImageField_ShouldNotRemoveLineBreakInBackText_IfItIsBetweenTextFragments()
    {
        // Arrange
        var backText = "<img src=\"a.jpg\"><br>to fear<br>to be afraid";
        var fields = AnkiNote.SerializeFields("", "", backText, "", "", "");
        var note = new AnkiNote(0, "template", "", fields);

        // Act
        MoveImageToImageField.MigrateImageToImageField(note);

        // Assert
        note.Image.Should().Be("<img src=\"a.jpg\">");
        note.BackText.Should().Be("to fear<br>to be afraid");
    }

    [DataTestMethod]
    [DataRow("<img src=\"a.jpg\" alt=\"some alt text\">to fear", "<img src=\"a.jpg\" alt=\"some alt text\">", "to fear")]
    [DataRow("<img src=\"a.jpg\" alt=\"some alt text\"/>to fear", "<img src=\"a.jpg\" alt=\"some alt text\">", "to fear")]
    [DataRow("<img src=\"a.jpg\" alt=\"some alt text\" />to fear", "<img src=\"a.jpg\" alt=\"some alt text\">", "to fear")]
    [DataRow("<img src=\"a.jpg\" alt=\"some alt text\" class=\"some-class\">to fear", "<img src=\"a.jpg\" alt=\"some alt text\" class=\"some-class\">", "to fear")]
    [DataRow("<img src=\"a.jpg\" alt=\"some alt text\" class=\"some-class\"/>to fear", "<img src=\"a.jpg\" alt=\"some alt text\" class=\"some-class\">", "to fear")]
    [DataRow("<img src=\"a.jpg\" alt=\"some alt text\" class=\"some-class\"/> to fear", "<img src=\"a.jpg\" alt=\"some alt text\" class=\"some-class\">", "to fear")]
    [DataRow("<img src=\"a.jpg\" alt=\"some alt text\" class=\"some-class\" />to fear", "<img src=\"a.jpg\" alt=\"some alt text\" class=\"some-class\">", "to fear")]
    [DataRow("<img src=\"a.jpg\" alt=\"some alt text\" class=\"some-class\" /> to fear", "<img src=\"a.jpg\" alt=\"some alt text\" class=\"some-class\">", "to fear")]
    public void MigrateImageToImageField_ShouldMoveImageWhileBeingFlexibleAboutImageTagPropertiesAndWhitespaceAndCasing(
        string frontText, string expectedImage, string expectedFrontText)
    {
        // Arrange
        var fields = AnkiNote.SerializeFields(frontText, "", "", "", "", "");
        var note = new AnkiNote(0, "template", "", fields);

        // Act
        MoveImageToImageField.MigrateImageToImageField(note);

        // Assert
        note.Image.Should().Be(expectedImage);
        note.FrontText.Should().Be(expectedFrontText);
    }
}
