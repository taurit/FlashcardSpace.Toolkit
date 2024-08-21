using BookToAnki.Services;
using FluentAssertions;

namespace BookToAnki.Tests;
[TestClass]
public class WordsLinkerTest
{
    [TestMethod]
    public void When_TwoWordsAreLinked_Expect_GetAllLinkedWordsReturnsAllWordsInAGroup()
    {
        // Arrange
        var sut = new WordsLinker();
        sut.LinkWords("word1", "word2");

        // Act
        var groupByWord1 = sut.GetAllLinkedWords("word1");
        var groupByWord2 = sut.GetAllLinkedWords("word2");

        // Assert
        groupByWord1.Should().BeEquivalentTo("word1", "word2");
        groupByWord2.Should().BeEquivalentTo("word1", "word2");
    }

    [TestMethod]
    public void When_TwoWordsAreLinked_Expect_IsLinkedReturnsTrueForBoth()
    {
        // Arrange
        var sut = new WordsLinker();
        sut.LinkWords("word1", "word2");

        // Act / Assert
        sut.IsWordLinkedWithAnyOther("word1").Should().BeTrue();
        sut.IsWordLinkedWithAnyOther("word2").Should().BeTrue();
    }

    [TestMethod]
    public void When_TwoGroupsOfWordsAreLinked_Expect_AllWordsAreLinkedWithEachOther()
    {
        // Arrange
        var sut = new WordsLinker();
        sut.LinkWords("word1", "word2");
        sut.LinkWords("word3", "word4");

        // Act
        sut.LinkWords("word2", "word3");

        // Assert
        sut.GetAllLinkedWords("word1").Should().BeEquivalentTo("word1", "word2", "word3", "word4");
        sut.GetAllLinkedWords("word2").Should().BeEquivalentTo("word1", "word2", "word3", "word4");
        sut.GetAllLinkedWords("word3").Should().BeEquivalentTo("word1", "word2", "word3", "word4");
        sut.GetAllLinkedWords("word4").Should().BeEquivalentTo("word1", "word2", "word3", "word4");
        sut.AreWordsLinked("word1", "word2").Should().BeTrue();
        sut.AreWordsLinked("word2", "word3").Should().BeTrue();
        sut.AreWordsLinked("word3", "word4").Should().BeTrue();
        sut.AreWordsLinked("word1", "word3").Should().BeTrue();
        sut.AreWordsLinked("word1", "word4").Should().BeTrue();
    }

    [TestMethod]
    public void When_WordGetsUnlinked_Expect_ItIsNotPartOfAnyWordGroup()
    {
        // Arrange
        var sut = new WordsLinker();
        sut.LinkWords("word1", "word2");
        sut.LinkWords("word2", "word3");

        // Act
        sut.UnlinkWord("word2");

        // Assert
        sut.IsWordLinkedWithAnyOther("word2").Should().BeFalse();
        sut.IsWordLinkedWithAnyOther("word1").Should().BeTrue();
        sut.IsWordLinkedWithAnyOther("word2").Should().BeFalse();
        sut.IsWordLinkedWithAnyOther("word3").Should().BeTrue(because: "A link between word1 and word3 should remain established");
    }


    [TestMethod]
    public void When_ThereIsJustOneWordLeftInAGroup_Expect_IsIsConsideredUnlinked()
    {
        // Arrange
        var sut = new WordsLinker();
        sut.LinkWords("word1", "word2");
        sut.IsWordLinkedWithAnyOther("word1").Should().BeTrue();
        sut.IsWordLinkedWithAnyOther("word2").Should().BeTrue();

        // Act
        sut.UnlinkWord("word1");

        // Assert
        sut.IsWordLinkedWithAnyOther("word1").Should().BeFalse(because: "It was explicitly unlinked");
        sut.IsWordLinkedWithAnyOther("word2").Should().BeFalse(because: "It was the only word left in a group");
    }
}
