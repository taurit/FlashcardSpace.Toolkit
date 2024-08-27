using BookToAnki.Models;
using BookToAnki.Services;
using CoreLibrary.Interfaces;

namespace BookToAnki.Tests;
[TestClass]
public class SentenceMatcherTests
{

    [TestMethod]
    public void When_DifferentWordsAreCompared_Expect_False()
    {
        // Arrange

        // Act
        var areEqual = SentenceMatcher.UkrainianWordEquals("aaa", "bbb");

        // Assert
        Assert.IsFalse(areEqual);

    }

    [TestMethod]
    public void When_SameWordsAreCompared_Expect_True()
    {
        // Arrange

        // Act
        var areEqual = SentenceMatcher.UkrainianWordEquals("гарному", "гарному");

        // Assert
        Assert.IsTrue(areEqual);
    }

    [TestMethod]
    public void When_EquivalentWordsAreCompared_Expect_True()
    {
        // Arrange

        // Act
        var areEqual1 = SentenceMatcher.UkrainianWordEquals("й", "і");
        var areEqual2 = SentenceMatcher.UkrainianWordEquals("і", "й");

        // Assert
        Assert.IsTrue(areEqual1);
        Assert.IsTrue(areEqual2);
    }

    [TestMethod]
    public void When_SentencesContainClearlyDifferentWords_Expect_NoMatch()
    {
        // Arrange
        var sentences = new List<Sentence>
        {
            new("Hello world", ["Hello", "world"])
        };

        var transcript = new Transcript([
            new("Tralalala", 0, 1),
            new("Alalalala", 2, 3)
        ], "irrelevant");

        // Act
        var matches = SentenceMatcher.Match(sentences, transcript);

        // Assert
        Assert.IsTrue(matches.Count == 0);
    }

    [TestMethod]
    public void When_SentencesContainIdenticalWords_Expect_Match()
    {
        // Arrange
        var sentences = new List<Sentence>
        {
            new("Hello world", ["Hello", "world"])
        };

        var transcript = new Transcript([
            new("Hello", 0, 1),
            new("world", 2, 3)
        ], "irrelevant");

        // Act
        var matches = SentenceMatcher.Match(sentences, transcript);

        // Assert
        Assert.IsTrue(matches.Count == 1);
        Assert.AreEqual(sentences[0], matches.First().Sentence);

        Assert.AreEqual(sentences[0].Words.First(), matches[0].WordsFromTranscript.First().Word);
        Assert.AreEqual(sentences[0].Words.Last(), matches[0].WordsFromTranscript.Last().Word);
    }

    [TestMethod]
    public void When_SentencesContainIdenticalWordsButDifferentCasing_Expect_Match()
    {
        // Arrange
        var sentences = new List<Sentence>
        {
            new("Hello world", ["Hello", "WoRlD"])
        };

        var transcript = new Transcript([
            new("HELLo", 0, 1),
            new("world", 0, 1)
        ], "irrelevant");

        // Act
        var matches = SentenceMatcher.Match(sentences, transcript);

        // Assert
        Assert.IsTrue(matches.Count == 1);
        Assert.AreEqual(sentences[0], matches.First().Sentence);

        Assert.AreEqual(sentences[0].Words.First(), matches[0].WordsFromTranscript.First().Word, true);
        Assert.AreEqual(sentences[0].Words.Last(), matches[0].WordsFromTranscript.Last().Word, true);
    }

    [TestMethod]
    public void When_SentencesContainIdenticalWordsInCyryllic_Expect_Match()
    {
        // Arrange
        var sentences = new List<Sentence>
        {
            new("Коли містер", ["Коли", "містер"])
        };

        var transcript = new Transcript([
            new("Коли", 0, 1),
            new("містер", 0, 1)
        ], "irrelevant");

        // Act
        var matches = SentenceMatcher.Match(sentences, transcript);

        // Assert
        Assert.IsTrue(matches.Count == 1);

        Assert.AreEqual(sentences[0].Words.First(), matches[0].WordsFromTranscript.First().Word);
        Assert.AreEqual(sentences[0].Words.Last(), matches[0].WordsFromTranscript.Last().Word);
    }

    // basic typo detection
    [TestMethod]
    public void When_SentencesContainLongWordsDifferingInOneLetterOnly_Expect_Match()
    {
        // Arrange
        var sentences = new List<Sentence>
        {
            new("Hello worldwide xxx", ["Hello", "worldwide", "xxx"])
        };

        var transcript = new Transcript([
            new("Hello", 0, 1),
            new("worldwise", 0, 1),
            new("xxx", 0, 1)
        ], "irrelevant");

        // Act
        var matches = SentenceMatcher.Match(sentences, transcript);

        // Assert
        Assert.IsTrue(matches.Count == 1);
        Assert.AreEqual(sentences[0], matches.First().Sentence);

        Assert.AreEqual(sentences[0].Words.First(), matches[0].WordsFromTranscript.First().Word);
        Assert.AreEqual(sentences[0].Words.Last(), matches[0].WordsFromTranscript.Last().Word);
    }

    [TestMethod]
    public void When_SentencesContainWordDifferingSignificantlyAtTheEnd_Expect_NoMatch()
    {
        // Arrange
        var sentences = new List<Sentence>
        {
            new("Hello sgdfgsgsdgsdfgdf", ["Hello", "sgdfgsgsdgsdfgdf"])
        };

        var transcript = new Transcript([
            new("Hello", 0, 1),
            new("world", 0, 1)
        ], "irrelevant");

        // Act
        var matches = SentenceMatcher.Match(sentences, transcript);

        // Assert
        Assert.IsTrue(matches.Count == 0);
    }

    // basic typo detection
    [TestMethod]
    public void When_SentencesContainWordDifferingSignificantlyInTheMiddle_Expect_NoMatch()
    {
        // Arrange
        var sentences = new List<Sentence>
        {
            new("Hello sgdfgsgsdgsdfgdf hi", ["Hello", "sgdfgsgsdgsdfgdf", "hi"])
        };

        var transcript = new Transcript([
            new("Hello", 0, 1),
            new("world", 0, 1),
            new("hi", 0, 1)
        ], "irrelevant");

        // Act
        var matches = SentenceMatcher.Match(sentences, transcript);

        // Assert
        Assert.IsTrue(matches.Count == 0);
    }

    // basic typo detection
    [TestMethod]
    public void When_TranscriptContainsOneAdditionalWordInsideTheSentence_Expect_Match()
    {
        // Arrange
        var sentences = new List<Sentence>
        {
            new("Натомість місіс Дурслі була худорлява", ["Натомість", "місіс", "Дурслі", "була", "худорлява"])
        };

        var transcript = new Transcript([
            new("Натомість", 0, 1),
            new("місіс", 0, 1),

            // this happens in a transcript sometimes
            new("Дур", 0, 1),
            new("слі", 0, 1),

            new("була", 0, 1),
            new("худорлява", 0, 1)
        ], "irrelevant");

        // Act
        var matches = SentenceMatcher.Match(sentences, transcript);

        // Assert
        Assert.IsTrue(matches.Count == 1);
        Assert.AreEqual(sentences[0], matches.First().Sentence);

        Assert.AreEqual(sentences[0].Words.First(), matches[0].WordsFromTranscript.First().Word);
        Assert.AreEqual(sentences[0].Words.Last(), matches[0].WordsFromTranscript.Last().Word);
    }

    [TestMethod]
    public void When_TranscriptContainsOneAdditionalWordInsideTheSentence2_Expect_Match()
    {
        // Arrange
        var sentences = new List<Sentence>
        {
            new("Навпаки, він радісно заусміхався й верескнув.", ["Навпаки", "він", "радісно", "заусміхався", "й", "верескнув"])
        };

        var transcript = new Transcript([
            new("asdasd", 0, 1),
            new("asdasdasd", 0, 1),

            new("навпаки", 0, 1),
            new("він", 0, 1),
            new("радісно", 0, 1),

            // happened
            new("за", 0, 1),
            new("усміхався", 0, 1),

            new("і", 0, 1),
            new("верескнув", 0, 1),
            new("asdasd", 0, 1)
        ], "irrelevant");

        // Act
        var matches = SentenceMatcher.Match(sentences, transcript);

        // Assert
        Assert.IsTrue(matches.Count == 1);
        Assert.AreEqual(sentences[0], matches.First().Sentence);

        Assert.AreEqual(sentences[0].Words.First(), matches[0].WordsFromTranscript.First().Word, true);
        Assert.AreEqual(sentences[0].Words.Last(), matches[0].WordsFromTranscript.Last().Word, true);
    }

    [TestMethod]
    public void When_TranscriptContainsErrorInLastWord_Expect_NoMatch()
    {
        // Arrange
        var sentences = new List<Sentence>
        {
            new("Той хлопчик був ще однією причиною не знатися з Поттерами",
                [
                    "Той",
                    "хлопчик",
                    "був",
                    "ще",
                    "однією",
                    "причиною",
                    "не",
                    "знатися",
                    "з",
                    "Поттерами"
                ]
            )
        };

        var transcript = new Transcript([
            new("той", 0, 1),
            new("хлопчик", 0, 1),
            new("був", 0, 1),
            new("ще", 0, 1),
            new("однією", 0, 1),
            new("причиною", 0, 1),
            new("не", 0, 1),

            new("знатися", 0, 1),
            new("споттерами", 0, 1),
            new("Дурслі", 0, 1),
            new("не", 0, 1),
            new("xxx", 0, 1)
        ], "irrelevant");

        // Act
        var matches = SentenceMatcher.Match(sentences, transcript);

        // Assert
        Assert.IsTrue(matches.Count == 0);
    }
}

