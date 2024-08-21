using BookToAnki.Models;
using BookToAnki.Models.GoogleCloudTranscripts;
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
            new Sentence("Hello world", new List<string> { "Hello", "world" })
        };

        var transcript = new Transcript(new List<GoogleCloudTranscriptWord> {
            new GoogleCloudTranscriptWord("Tralalala", 0, 1),
            new GoogleCloudTranscriptWord("Alalalala", 2, 3)
        }, "irrelevant");

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
            new Sentence("Hello world", new List<string> { "Hello", "world" })
        };

        var transcript = new Transcript(new List<GoogleCloudTranscriptWord> {
            new GoogleCloudTranscriptWord("Hello", 0, 1),
            new GoogleCloudTranscriptWord("world", 2, 3)
        }, "irrelevant");

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
            new Sentence("Hello world", new List<string> { "Hello", "WoRlD" })
        };

        var transcript = new Transcript(new List<GoogleCloudTranscriptWord> {
            new GoogleCloudTranscriptWord("HELLo", 0, 1),
            new GoogleCloudTranscriptWord("world", 0, 1),
        }, "irrelevant");

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
            new Sentence("Коли містер", new List<string> { "Коли", "містер" })
        };

        var transcript = new Transcript(new List<GoogleCloudTranscriptWord> {
            new GoogleCloudTranscriptWord("Коли", 0, 1),
            new GoogleCloudTranscriptWord("містер", 0, 1),
        }, "irrelevant");

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
            new Sentence("Hello worldwide xxx", new List<string> { "Hello", "worldwide", "xxx" })
        };

        var transcript = new Transcript(new List<GoogleCloudTranscriptWord> {
            new GoogleCloudTranscriptWord("Hello", 0, 1),
            new GoogleCloudTranscriptWord("worldwise", 0, 1),
            new GoogleCloudTranscriptWord("xxx", 0, 1),
        }, "irrelevant");

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
            new Sentence("Hello sgdfgsgsdgsdfgdf", new List<string> { "Hello", "sgdfgsgsdgsdfgdf" })
        };

        var transcript = new Transcript(new List<GoogleCloudTranscriptWord> {
            new GoogleCloudTranscriptWord("Hello", 0, 1),
            new GoogleCloudTranscriptWord("world", 0, 1),
        }, "irrelevant");

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
            new Sentence("Hello sgdfgsgsdgsdfgdf hi", new List<string> { "Hello", "sgdfgsgsdgsdfgdf", "hi" })
        };

        var transcript = new Transcript(new List<GoogleCloudTranscriptWord> {
            new GoogleCloudTranscriptWord("Hello", 0, 1),
            new GoogleCloudTranscriptWord("world", 0, 1),
            new GoogleCloudTranscriptWord("hi", 0, 1),
        }, "irrelevant");

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
            new Sentence("Натомість місіс Дурслі була худорлява", new List<string> { "Натомість", "місіс", "Дурслі", "була", "худорлява" })
        };

        var transcript = new Transcript(new List<GoogleCloudTranscriptWord> {
            new GoogleCloudTranscriptWord("Натомість", 0, 1),
            new GoogleCloudTranscriptWord("місіс", 0, 1),

            // this happens in a transcript sometimes
            new GoogleCloudTranscriptWord("Дур", 0, 1),
            new GoogleCloudTranscriptWord("слі", 0, 1),

            new GoogleCloudTranscriptWord("була", 0, 1),
            new GoogleCloudTranscriptWord("худорлява", 0, 1),
        }, "irrelevant");

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
            new Sentence("Навпаки, він радісно заусміхався й верескнув.", new List<string> { "Навпаки", "він", "радісно", "заусміхався", "й", "верескнув" })
        };

        var transcript = new Transcript(new List<GoogleCloudTranscriptWord> {
            new GoogleCloudTranscriptWord("asdasd", 0, 1),
            new GoogleCloudTranscriptWord("asdasdasd", 0, 1),

            new GoogleCloudTranscriptWord("навпаки", 0, 1),
            new GoogleCloudTranscriptWord("він", 0, 1),
            new GoogleCloudTranscriptWord("радісно", 0, 1),

            // happened
            new GoogleCloudTranscriptWord("за", 0, 1),
            new GoogleCloudTranscriptWord("усміхався", 0, 1),

            new GoogleCloudTranscriptWord("і", 0, 1),
            new GoogleCloudTranscriptWord("верескнув", 0, 1),
            new GoogleCloudTranscriptWord("asdasd", 0, 1),
        }, "irrelevant");

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
            new Sentence("Той хлопчик був ще однією причиною не знатися з Поттерами",
            new List<string>
            {
                "Той",
                "хлопчик",
                "був",
                "ще",
                "однією",
                "причиною",
                "не",
                "знатися",
                "з",
                "Поттерами",
                }
            )
        };

        var transcript = new Transcript(new List<GoogleCloudTranscriptWord> {
            new GoogleCloudTranscriptWord("той", 0, 1),
            new GoogleCloudTranscriptWord("хлопчик", 0, 1),
            new GoogleCloudTranscriptWord("був", 0, 1),
            new GoogleCloudTranscriptWord("ще", 0, 1),
            new GoogleCloudTranscriptWord("однією", 0, 1),
            new GoogleCloudTranscriptWord("причиною", 0, 1),
            new GoogleCloudTranscriptWord("не", 0, 1),

            new GoogleCloudTranscriptWord("знатися", 0, 1),
            new GoogleCloudTranscriptWord("споттерами", 0, 1),
            new GoogleCloudTranscriptWord("Дурслі", 0, 1),
            new GoogleCloudTranscriptWord("не", 0, 1),
            new GoogleCloudTranscriptWord("xxx", 0, 1),
        }, "irrelevant");

        // Act
        var matches = SentenceMatcher.Match(sentences, transcript);

        // Assert
        Assert.IsTrue(matches.Count == 0);
    }
}

