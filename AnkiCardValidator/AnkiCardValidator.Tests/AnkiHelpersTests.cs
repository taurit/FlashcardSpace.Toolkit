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
        var result = AnkiHelpers.ParseTags(tags);

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
        var result = AnkiHelpers.ParseTags(tags);

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
        var result = AnkiHelpers.ParseTags(tags);

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
        var result = AnkiHelpers.ParseTags(tags);

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
        var result = AnkiHelpers.ParseTags(tags);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain("tag1");
        result.Should().Contain("tag2");
    }
}
