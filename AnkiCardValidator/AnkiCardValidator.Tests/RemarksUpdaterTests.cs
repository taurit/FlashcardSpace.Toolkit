using AnkiCardValidator.Utilities;
using FluentAssertions;

namespace AnkiCardValidator.Tests;

[TestClass]
public class RemarksUpdaterTests
{
    [TestMethod]
    public void AddRemark_WhenRemarkStringWasEmpty_RemarkIsAdded()
    {
        // Act
        var result = "".AddOrUpdateRemark("scope1", "test remark 1");

        // Assert
        result.Should().Be("<div class=\"scope1\">test remark 1</div>");
    }

    [TestMethod]
    public void AddRemark_WhenRemarkStringWasNull_RemarkIsAdded()
    {
        // Act
        var result = ((string?)null).AddOrUpdateRemark("scope1", "test remark 1");

        // Assert
        result.Should().Be("<div class=\"scope1\">test remark 1</div>");
    }

    [TestMethod]
    public void AddRemark_WhenRemarkStringsAddedTwice_OnlyTheSecondValueRemains()
    {
        // Act
        var result = "".AddOrUpdateRemark("scope", "test remark 1");
        var result2 = result.AddOrUpdateRemark("scope", "test remark 2");

        // Assert
        result2.Should().Be("<div class=\"scope1\">test remark 2</div>");
    }

    [TestMethod]
    public void AddRemark_WhenRemarkStringsAreAddedToIndependentClasses_BothAreRetained()
    {
        // Act
        var result = "".AddOrUpdateRemark("scope1", "test remark 1");
        var result2 = result.AddOrUpdateRemark("scope2", "test remark 2");

        // Assert
        result2.Should().Contain("<div class=\"scope1\">test remark 1</div>");
        result2.Should().Contain("<div class=\"scope2\">test remark 2</div>");
    }

    [TestMethod]
    public void AddRemark_WhenRemarkIsUpdated_OtherRemarksRemainUnchanged()
    {
        // Arrange
        var result = "".AddOrUpdateRemark("scope1", "test remark 1");
        var result2 = result.AddOrUpdateRemark("scope2", "test remark 2");

        // Act
        var result3 = result2.AddOrUpdateRemark("scope1", "test remark 1 changed");

        // Assert
        result3.Should().Contain("<div class=\"scope1\">test remark 1 changed</div>");
        result3.Should().Contain("<div class=\"scope2\">test remark 2</div>");
        result3.Should().NotContain("<div class=\"scope1\">test remark 1</div>");
    }


    [TestMethod]
    public void AddRemark_WhenRemarkContainingHtmlTagsIsUpdated_OtherRemarksRemainUnchanged()
    {
        // Arrange
        var result = "".AddOrUpdateRemark("scope1", "<div class=\"inner\">test</div> <div class=\"inner1a\"><div></div>remark 1</div>");
        var result2 = result.AddOrUpdateRemark("scope2", "test <div class=\"inner2\">test</div> remark 2");

        // Act
        var result3 = result2.AddOrUpdateRemark("scope1", "test <b>remark</b> 1 changed");

        // Assert
        result3.Should().Contain("<div class=\"scope1\">test <b>remark</b> 1 changed</div>");
        result3.Should().Contain("<div class=\"scope2\">test <div class=\"inner2\">test</div> remark 2</div>");
        result3.Should().NotContain("<div class=\"scope1\"><div class=\"inner\">test</div> <div class=\"inner1a\"><div></div>remark 1</div></div>");
    }

    [TestMethod]
    public void RemoveRemark_WhenRemarkIsRemoved_OtherRemarksRemainUnchanged()
    {
        // Arrange
        var result = "".AddOrUpdateRemark("scope1", "test remark 1");
        var result2 = result.AddOrUpdateRemark("scope2", "test remark 2");

        // Act
        var result3 = result2.RemoveRemark("scope1");

        // Assert
        result3.Should().NotContain("<div class=\"scope1\">test remark 1</div>");
        result3.Should().Contain("<div class=\"scope2\">test remark 2</div>");
    }

}
