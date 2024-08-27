using BookToAnki.Services;
using CoreLibrary.Interfaces;
using FluentAssertions;

namespace BookToAnki.Tests;
[TestClass]
public class WordStatisticCounterTests
{
    [TestMethod]
    public void When_SingleWordsAreRequestedForEmptySentence_Expect_EmptyListOfWordsInOutput()
    {
        // Arrange
        var sut = new WordStatisticCounter(null);

        var sentence = new Sentence("", []);

        // Act
        var wordGroups = sut.GetWordGroups(sentence);

        // Assert
        Assert.IsNotNull(wordGroups);
        Assert.IsTrue(wordGroups.Count == 0);
    }

    [TestMethod]
    public void When_SingleWordsAreRequested_Expect_SameWordsAsInInput()
    {
        // Arrange
        var sut = new WordStatisticCounter(null);

        var sentence = new Sentence("irrelevant", ["Word1", "word2"]);

        // Act
        var wordGroups = sut.GetWordGroups(sentence);

        // Assert
        Assert.IsNotNull(wordGroups);
        Assert.AreEqual("Word1", wordGroups[0]);
        Assert.AreEqual("word2", wordGroups[1]);
    }


    [TestMethod]
    public void When_TwoSubsequentRequestAreDone_Expect_NoInterferenceBetweenThem()
    {
        // Arrange
        var sut = new WordStatisticCounter(null);

        var sentence1 = new Sentence("irrelevant", ["word1", "word2"]);
        var sentence2 = new Sentence("irrelevant", ["word3", "word4"]);

        // Act
        var wordGroups1 = sut.GetWordGroups(sentence1);
        var wordGroups2 = sut.GetWordGroups(sentence2);

        // Assert
        Assert.IsNotNull(wordGroups1);
        wordGroups1.Should().HaveCount(2);
        wordGroups1.Should().Contain("word1");
        wordGroups1.Should().Contain("word2");

        Assert.IsNotNull(wordGroups2);
        wordGroups2.Should().HaveCount(2);
        wordGroups2.Should().Contain("word3");
        wordGroups2.Should().Contain("word4");
    }

}
