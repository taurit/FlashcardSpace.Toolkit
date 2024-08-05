using AnkiCardValidator.Utilities;
using FluentAssertions;

namespace AnkiCardValidator.Tests;

[TestClass]
public class AnkiHelpersTests
{
    [TestMethod]
    public void WhenTagsAreParsed_AllTagsAreReturned()
    {
        // Arrange
        var tags = " tag1 tag2 tag3 tag4 tag5 ";

        // Act
        var result = AnkiTagHelpers.ParseTags(tags);

        // Assert
        result.Should().HaveCount(5);
        result.Should().Contain("tag1");
        result.Should().Contain("tag2");
        result.Should().Contain("tag3");
        result.Should().Contain("tag4");
        result.Should().Contain("tag5");
    }

    [TestMethod]
    public void WhenTagsAreParsed_DuplicateTagsAreRemoved()
    {
        // Arrange
        var tags = " tag1 tag2 tag3 tag1 tag2 tag3 ";

        // Act
        var result = AnkiTagHelpers.ParseTags(tags);

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain("tag1");
        result.Should().Contain("tag2");
        result.Should().Contain("tag3");
    }

    [TestMethod]
    public void WhenTagsAreParsed_ParsingWorksEvenWithoutLeadingOrTrailingSpace()
    {
        // Arrange
        var tags = "tag1 tag2";

        // Act
        var result = AnkiTagHelpers.ParseTags(tags);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain("tag1");
        result.Should().Contain("tag2");
    }

    [DataTestMethod]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow("  ")]
    public void WhenTagsAreParsed_EmptyListOfTagsResultsInEmptyList(string tags)
    {
        // Arrange
        // Act
        var result = AnkiTagHelpers.ParseTags(tags);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [DataTestMethod]
    [DataRow("tag1 tag2")]
    [DataRow(" tag1 tag2 ")]
    [DataRow("  tag1 tag2  ")]
    [DataRow("  tag1  tag2  ")]
    [DataRow("  tag1   tag2  ")]
    [DataRow("  tag1    tag2  ")]
    public void WhenTagsAreParsed_MultipleSpacesAreTreatedSameAsSingleSpaces(string tags)
    {
        // Arrange

        // Act
        var result = AnkiTagHelpers.ParseTags(tags);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain("tag1");
        result.Should().Contain("tag2");
    }

    [DataTestMethod]
    [DataRow("tag1 tag2", "tag3", " tag1 tag2 tag3 ")]
    [DataRow(" tag1 tag2 ", "tag3", " tag1 tag2 tag3 ")]
    [DataRow(" tag1 tag2 ", "tag2", " tag1 tag2 ")]
    [DataRow(" tag2 tag1 ", "tag2", " tag2 tag1 ")]
    [DataRow("", "tag2", " tag2 ")]
    [DataRow(" ", "tag2", " tag2 ")]
    [DataRow("  ", "tag2", " tag2 ")]
    [DataRow("tag2", "tag2", " tag2 ")]
    [DataRow("tag2", "tag-2", " tag2 tag-2 ")]
    public void WhenAddingTag_TagIsAddedAtTheEndIfNotAlreadyPresent(string existingTags, string tagToAdd, string expectedResult)
    {
        // Arrange
        // Act
        var result = AnkiTagHelpers.AddTagToAnkiTagsString(tagToAdd, existingTags);

        // Assert
        result.Should().Be(expectedResult);
    }

    [TestMethod]
    public void WhenAddingTagToAnkiTagsString_TagMustNotContainSpaces()
    {
        // Arrange
        var tagToAdd = "tag with spaces";
        var tagsString = "tag1 tag2 tag3";

        // Act
        Action act = () => AnkiTagHelpers.AddTagToAnkiTagsString(tagToAdd, tagsString);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("Tag must not contain spaces or special characters.");
    }

    [TestMethod]
    public void WhenAddingTagToAnkiTagsString_TagMustNotContainSpecialCharacters()
    {
        // Arrange
        var tagToAdd = "tag%with$special@characters";
        var tagsString = "tag1 tag2 tag3";

        // Act
        Action act = () => AnkiTagHelpers.AddTagToAnkiTagsString(tagToAdd, tagsString);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("Tag must not contain spaces or special characters.");
    }
}
