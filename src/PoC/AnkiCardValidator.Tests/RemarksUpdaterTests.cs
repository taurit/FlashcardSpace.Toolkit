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
        result2.Should().Be("<div class=\"scope\">test remark 2</div>");
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
    public void AddRemark_WhenRemarkIsAddedToStringWithInvalidMarkup_ReturnOriginalString()
    {
        // Arrange
        var originalRemarks = "<div class=\"scope\">THIS TAG IS NOT CLOSED";

        // Act
        var afterAddition = originalRemarks.AddOrUpdateRemark("scope", "test remark 1");

        // Assert
        afterAddition.Should().Be(originalRemarks);
    }

    [TestMethod]
    public void RemoveRemark_WhenRemarkIsRemoved_OtherRemarksRemainUnchanged()
    {
        // Arrange

        var result = "".AddOrUpdateRemark("scope1", "test remark 1");
        var result2 = result.AddOrUpdateRemark("scope2", "test remark 2");
        var result3 = result2.AddOrUpdateRemark("scope3", "test remark 3");


        // Act
        var afterRemoval = result3.RemoveRemark("scope2");

        // Assert
        afterRemoval.Should().NotContain("<div class=\"scope2\">test remark 2</div>");
        afterRemoval.Should().Contain("<div class=\"scope1\">test remark 1</div>");
        afterRemoval.Should().Contain("<div class=\"scope3\">test remark 3</div>");
    }

    [TestMethod]
    public void RemoveRemark_WhenRemarkIsRemovedFromNullString_DoNotCrash()
    {
        // Act
        var afterRemoval = ((string)null!).RemoveRemark("scope");

        // Assert
        afterRemoval.Should().BeEmpty();
    }

    [TestMethod]
    public void RemoveRemark_WhenRemarkIsRemovedFromEmptyString_DoNotCrash()
    {
        // Act
        var afterRemoval = "".RemoveRemark("scope");

        // Assert
        afterRemoval.Should().BeEmpty();
    }

    [TestMethod]
    public void RemoveRemark_WhenRemarkIsRemovedFromStringWithInvalidMarkup_ReturnOriginalString()
    {
        // Act
        var originalRemarks = "<div class=\"scope\">THIS TAG IS NOT CLOSED";
        var afterRemoval = originalRemarks.RemoveRemark("scope");

        // Assert
        afterRemoval.Should().Be(originalRemarks);
    }

    [DataTestMethod]
    [DataRow("<div class=\"scope\">yes</div>", "scope")]
    [DataRow("<div class=\"scope unrelatedClass\">yes</div>", "scope")]
    [DataRow("<div class=\"scope-with-dash\">yes</div>", "scope-with-dash")]
    [DataRow("<div    class=\" scope-with-whitespaces  \">yes</div>", "scope-with-whitespaces")]
    [DataRow("<div style=\"\" class=\" scope-with-other-attributes \">yes</div>", "scope-with-other-attributes")]
    public void HasRemark_ShouldReturnTrueWhenHtmlContainsDivWithSpecifiedClass(string remarks, string scopeToCheck)
    {
        remarks.HasRemark(scopeToCheck).Should().BeTrue();
    }

    [DataTestMethod]
    [DataRow("<div class=\"scope\">no</div>", "scope2")]
    [DataRow("<div class=\"scope unrelatedClass\">no</div>", "scope2")]
    [DataRow("<div class=\"scope-with-dash\">TAG NOT CLOSED", "scope-with-dash")]
    public void HasRemark_ShouldReturnFalseWhenHtmlDoesNotContainCorrectDivWithSpecifiedClass(string remarks, string scopeToCheck)
    {
        remarks.HasRemark(scopeToCheck).Should().BeFalse();
    }

    [DataTestMethod]
    [DataRow("aaabbb<div class=\"scope\">yes</div>", "<div class=\"scope\">yes</div>")]
    [DataRow("<div class=\"scope unrelatedClass\">yes</div>aaabbb", "<div class=\"scope unrelatedClass\">yes</div>")]
    public void TryGetRemark_ShouldReturnRemarkTagIfPresent(string remarks, string expected)
    {
        remarks.TryGetRemark("scope").Should().Be(expected);
    }

    [DataTestMethod]
    [DataRow("aaabbb<div class=\"scope\">yes</div>")]
    [DataRow("<div class=\"scope unrelatedClass\">yes</div>aaabbb")]
    public void TryGetRemark_ShouldReturnNullTagIfRemarkNotPresent(string remarks)
    {
        remarks.TryGetRemark("scopeThatDoesntExist").Should().BeNull();
    }

}
