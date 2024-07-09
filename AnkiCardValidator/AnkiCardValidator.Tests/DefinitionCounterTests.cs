using AnkiCardValidator.Utilities;
using FluentAssertions;

namespace AnkiCardValidator.Tests;

[TestClass]
public class DefinitionCounterTests
{
    [TestMethod]
    public void CountDefinitions_ShouldReturnZero_WhenWordIsEmpty()
    {
        // Arrange
        var definitionCounter = new DefinitionCounter();

        // Act
        int result = definitionCounter.CountDefinitions("");

        // Assert
        result.Should().Be(0);
    }

    [TestMethod]
    public void CountDefinitions_ShouldReturnOne_WhenWordHasSingleDefinition()
    {
        // Arrange
        var definitionCounter = new DefinitionCounter();

        // Act
        int result = definitionCounter.CountDefinitions("definition");

        // Assert
        result.Should().Be(1);
    }

    [TestMethod]
    public void CountDefinitions_ShouldReturnTwo_WhenWordHasTwoDefinitions()
    {
        // Arrange
        var definitionCounter = new DefinitionCounter();

        // Act
        int result = definitionCounter.CountDefinitions("definition1,definition2");

        // Assert
        result.Should().Be(2);
    }

    [TestMethod]
    public void CountDefinitions_ShouldReturnThree_WhenWordHasThreeDefinitions()
    {
        // Arrange
        var definitionCounter = new DefinitionCounter();

        // Act
        int result = definitionCounter.CountDefinitions("definition1,definition2,definition3");

        // Assert
        result.Should().Be(3);
    }

    [TestMethod]
    public void CountDefinitions_ShouldDiscardEverythingInParentheses()
    {
        // Arrange
        var definitionCounter = new DefinitionCounter();

        // Act
        int result = definitionCounter.CountDefinitions("orzech (ogólnie, <i>nut</i>, nie jakiś specyficzny)");

        // Assert
        result.Should().Be(1);
    }

    [TestMethod]
    public void CountDefinitions_ShouldDiscardComasInHtmlTags()
    {
        // Arrange
        var definitionCounter = new DefinitionCounter();

        // Act
        int result = definitionCounter.CountDefinitions("orzech <span d-test='a,b,c,d'>włoski</span>, dwa kokoski");

        // Assert
        result.Should().Be(2);
    }
}
