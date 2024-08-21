using BookToAnki.Services;

namespace BookToAnki.Tests;
[TestClass]
public class StringDistanceTests
{
    [TestMethod]
    public void When_StringsAreIdentical_Expect_True()
    {
        // Arrange
        string str1 = "aaaa";
        string str2 = "aaaa";

        // Act
        var result = StringDistance.AreStringsVerySimilar(str1, str2);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void When_StringsAreOneEditApart_Expect_True()
    {
        // Arrange
        string str1 = "aaaa";
        string str2 = "aaab";

        // Act
        var result = StringDistance.AreStringsVerySimilar(str1, str2);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void When_LongStringsAreTwoEditsApart_Expect_True()
    {
        // Arrange
        string str1 = "aqqaaa";
        string str2 = "aqqabb";

        // Act
        var result = StringDistance.AreStringsVerySimilar(str1, str2);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void When_StringsAreMoreThanTwoEditsApart_Expect_False()
    {
        // Arrange
        string str1 = "aaaa";
        string str2 = "bbbb";

        // Act
        var result = StringDistance.AreStringsVerySimilar(str1, str2);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void When_OneStringIsEmpty_Expect_False()
    {
        // Arrange
        string str1 = "";
        string str2 = "aaaa";

        // Act
        var result = StringDistance.AreStringsVerySimilar(str1, str2);

        // Assert
        Assert.IsFalse(result);
    }

}
