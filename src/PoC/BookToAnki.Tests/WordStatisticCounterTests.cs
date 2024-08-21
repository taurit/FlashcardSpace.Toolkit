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

        var sentence = new Sentence("", new List<string>());

        // Act
        var wordGroups = sut.GetWordGroups(sentence, 1);

        // Assert
        Assert.IsNotNull(wordGroups);
        Assert.IsTrue(wordGroups.Count == 0);
    }

    [TestMethod]
    public void When_SingleWordsAreRequested_Expect_SameWordsAsInInput()
    {
        // Arrange
        var sut = new WordStatisticCounter(null);

        var sentence = new Sentence("irrelevant", new List<string> { "Word1", "word2" });

        // Act
        var wordGroups = sut.GetWordGroups(sentence, 1);

        // Assert
        Assert.IsNotNull(wordGroups);
        Assert.AreEqual("Word1", wordGroups[0]);
        Assert.AreEqual("word2", wordGroups[1]);
    }

    [TestMethod]
    public void When_WordPairsAreRequested_Expect_AllPairsOfWordsConcatenatedBySpaceCharacter()
    {
        // Arrange
        var sut = new WordStatisticCounter(null);

        var sentence = new Sentence("irrelevant", new List<string> { "Word1", "word2", "word3" });

        // Act
        var wordGroups = sut.GetWordGroups(sentence, 2);

        // Assert
        Assert.IsNotNull(wordGroups);
        Assert.AreEqual("Word1 word2", wordGroups[0]);
        Assert.AreEqual("word2 word3", wordGroups[1]);
    }


    [TestMethod]
    public void When_TwoSubsequestRequestAreDone_Expect_NoInterferenceBetweenThem()
    {
        // Arrange
        var sut = new WordStatisticCounter(null);

        var sentence1 = new Sentence("irrelevant", new List<string> { "word1", "word2" });
        var sentence2 = new Sentence("irrelevant", new List<string> { "word3", "word4" });

        // Act
        var wordGroups1 = sut.GetWordGroups(sentence1, 1);
        var wordGroups2 = sut.GetWordGroups(sentence2, 1);

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


    [TestMethod]
    public void When_WordTriplesAreRequested_Expect_CorrectTriples()
    {
        // Arrange
        var sut = new WordStatisticCounter(null);

        var sentence = new Sentence("irrelevant", new List<string> { "Word1", "word2", "word3", "word4" });

        // Act
        var wordGroups = sut.GetWordGroups(sentence, 3);

        // Assert
        Assert.IsNotNull(wordGroups);
        Assert.AreEqual("Word1 word2 word3", wordGroups[0]);
        Assert.AreEqual("word2 word3 word4", wordGroups[1]);
    }

    [TestMethod]
    public void When_WordPairsAreRequestedWhenThereIsJustOneWord_Expect_EmptyListOnOutput()
    {
        // Arrange
        var sut = new WordStatisticCounter(null);

        var sentence = new Sentence("irrelevant", new List<string> { "Word1" });

        // Act
        var wordGroups = sut.GetWordGroups(sentence, 2);

        // Assert
        wordGroups.Should().NotBeNull();
        wordGroups.Should().BeEmpty();
    }
}
