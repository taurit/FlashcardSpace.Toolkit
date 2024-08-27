using BookToAnki.Models;
using CoreLibrary.Interfaces;
using FluentAssertions;

namespace BookToAnki.Tests;

[TestClass]
public class WordDataTests
{
    [TestMethod]
    public void When_TwoDifferentWordsAreMerged_Expect_Exception()
    {
        // Arrange
        var sentence1 = new Sentence("Word1 is awesome", ["Word1", "is", "awesome"]);
        var sentence2 = new Sentence("Word2 is awesome", ["Word2", "is", "awesome"]);

        var word1 = new WordData("Word1", 2, [new("Word1", sentence1, [], null, null, null, null, null)]);

        var word2 = new WordData("Word2", 15, [new("Word2", sentence2, [], null, null, null, null, null)]);

        // Act
        Action act = () =>
        {
            var result = word1 + word2;
            Assert.Fail($"Expected exception, but got {result}");
        };

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [TestMethod]
    public void When_TwoIdenticalWordsAreMerged_Expect_ResultHasASumOfUsageExamples()
    {
        // Arrange
        var sentence1 = new Sentence("Word1 is awesome", ["Word1", "is", "awesome"]);
        var sentence2 = new Sentence("Word1 is awesome two", ["Word1", "is", "awesome", "two"]);

        var word1 = new WordData("Word1", 2, [new("Word1", sentence1, [], null, null, null, null, null)]);

        var word2 = new WordData("Word1", 15, [new("Word1", sentence2, [], null, null, null, null, null)]);

        // Act
        var result = word1 + word2;

        // Assert
        result.Occurrences.Should().Be(2 + 15);
        result.Word.Should().Be("Word1");
        result.UsageExamples.Should().HaveCount(2);
        result.UsageExamples.Should().Contain(x => x.Sentence == sentence1);
        result.UsageExamples.Should().Contain(x => x.Sentence == sentence2);
    }

    [TestMethod]
    public void When_TwoWordsDifferingByCasingAreMerged_Expect_ResultHasASumOfUsageExamples()
    {
        // Arrange
        var sentence1 = new Sentence("WORD1 is awesome", ["WORD1", "is", "awesome"]);
        var sentence2 = new Sentence("word1 is awesome two", ["word1", "is", "awesome", "two"]);

        var word1 = new WordData("WORD1", 2, [new("Word1", sentence1, [], null, null, null, null, null)]);

        var word2 = new WordData("word1", 15, [new("Word1", sentence2, [], null, null, null, null, null)]);

        // Act
        var result = word1 + word2;

        // Assert
        result.Occurrences.Should().Be(2 + 15);
        result.Word.Should().Be("word1"); // because it has more occurrences
        result.UsageExamples.Should().HaveCount(2);
        result.UsageExamples.Should().Contain(x => x.Sentence == sentence1);
        result.UsageExamples.Should().Contain(x => x.Sentence == sentence2);
    }

    [TestMethod]
    public void When_UsageExamplesFromTwoBooksAreMerged_Expect_AudioSamplesFromBothArePreserved()
    {
        // Arrange
        var sentence1 = new Sentence("Word1 is awesome", ["Word1", "is", "awesome"]);
        var sentence2 = new Sentence("Word1 is awesome", ["Word1", "is", "awesome"]);

        var word1 = new WordData("Word1", 1, [
            new("Word1", sentence1, [new(sentence1, null, "BOOK1.mp3")], null, null, null, null, null)
        ]);

        var word2 = new WordData("Word1", 1, [
            new("Word1", sentence2, [new(sentence2, null, "BOOK2.mp3")], null, null, null, null, null)
        ]);

        // Act
        var result = word1 + word2;

        // Assert
        result.Occurrences.Should().Be(1 + 1);
        result.Word.Should().Be("Word1");
        result.UsageExamples.Should().HaveCount(1); // because usage example sentence is the same in both
        result.UsageExamples.Single().Sentence.Text.Should().Be("Word1 is awesome");
        result.UsageExamples.Single().TranscriptMatches.Should().HaveCount(2);
        result.UsageExamples.Single().TranscriptMatches.Should().Contain(x => x.PathToAudioFile == "BOOK1.mp3");
        result.UsageExamples.Single().TranscriptMatches.Should().Contain(x => x.PathToAudioFile == "BOOK2.mp3");
    }


    [TestMethod]
    public void When_OneBookHasTranslationForUsageExampleButTheOtherDoesnt_Expect_ExistingTranslationIsAlwaysChosen()
    {
        // Arrange
        var sentence1 = new Sentence("Word1 is awesome", ["Word1", "is", "awesome"]);
        var sentence2 = new Sentence("Word1 is awesome", ["Word1", "is", "awesome"]);

        var word1 = new WordData("Word1", 1, [
            new("Word1", sentence1, [new(sentence1, null, "BOOK1.mp3")], null, null, "Existing Polish translation",
                null /* intentionally missing */, null)
        ]);

        var word2 = new WordData("Word1", 1, [
            new("Word1", sentence2, [new(sentence2, null, "BOOK2.mp3")], null, null, /* intentionally missing */ null,
                "Existing English translation", null)
        ]);

        // Act
        var result = word1 + word2;

        // Assert
        result.Occurrences.Should().Be(1 + 1);
        result.Word.Should().Be("Word1");
        result.UsageExamples.Should().HaveCount(1); // because usage example sentence is the same in both
        result.UsageExamples.Single().SentenceHumanTranslationPolish.Should().Be("Existing Polish translation");
        result.UsageExamples.Single().SentenceHumanTranslationEnglish.Should().Be("Existing English translation");
    }

    [TestMethod]
    public void When_GetPreferredCasingGetsTwoLowercaseWords_Expect_OneOfThemInResponse()
    {
        // Arrange
        var word1 = new WordData("cat", 10, []);
        var word2 = new WordData("cat", 20, []);

        // Act
        var result = WordData.GetPreferredCasing(word1, word2);

        // Assert
        result.Should().Be("cat");
    }

    [TestMethod]
    public void When_GetPreferredCasingGetsTwoMixedCaseWords_Expect_MorePopularVariantInResponse()
    {
        // Arrange
        var word1 = new WordData("Cat", 10, []);
        var word2 = new WordData("CAT", 20, []);

        var word3 = new WordData("Dog", 20, []);
        var word4 = new WordData("DOG", 10, []);

        // Act
        var result1 = WordData.GetPreferredCasing(word1, word2);
        var result2 = WordData.GetPreferredCasing(word3, word4);

        // Assert
        result1.Should().Be("CAT");
        result2.Should().Be("Dog");
    }

    [TestMethod]
    public void When_GetPreferredCasingGetsOneLowercaseOneMixedWord_Expect_LowercaseAlwaysWins()
    {
        // Arrange
        var word1 = new WordData("CAT", 20, []);
        var word2 = new WordData("cat", 1, []);

        // Act
        var result1 = WordData.GetPreferredCasing(word1, word2);
        var result2 = WordData.GetPreferredCasing(word2, word1);

        // Assert
        result1.Should().Be("cat");
        result2.Should().Be("cat");
    }
}
