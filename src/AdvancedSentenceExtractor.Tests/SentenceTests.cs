using AdvancedSentenceExtractor.Models;
using FluentAssertions;

namespace AdvancedSentenceExtractor.Tests;
[TestClass]
public class SentenceTests
{

    [TestMethod]
    public void When_ThereIsJustOneSentence_Expect_ItCanHaveNullReferencesToPreviousAndNextSentences()
    {
        // Arrange

        // Act
        var sentence = new Sentence("aaa", new List<string>() { "aaa" }, null);

        // Assert
        sentence.PreviousSentence.Should().BeNull();
    }

    [TestMethod]
    public void When_SentenceGetsReferenceToPreviousSentence_Expect_BothSentencesHaveReferencesUpdated()
    {
        // Arrange

        // Act
        var sentence1 = new Sentence("aaa", new List<string>() { "aaa" }, null);
        var sentence2 = new Sentence("bbb", new List<string>() { "bbb" }, sentence1);

        // Assert
        sentence1.PreviousSentence.Should().BeNull();
        sentence1.NextSentence.Should().Be(sentence2);
        sentence2.PreviousSentence.Should().Be(sentence1);
        sentence2.NextSentence.Should().Be(null);
    }
    [TestMethod]
    public void When_ThreeSentencesAreChained_Expect_CorrectReferences()
    {
        // Arrange

        // Act
        var sentence1 = new Sentence("aaa", new List<string>() { "aaa" }, null);
        var sentence2 = new Sentence("bbb", new List<string>() { "bbb" }, sentence1);
        var sentence3 = new Sentence("ccc", new List<string>() { "ccc" }, sentence2);

        // Assert
        sentence1.PreviousSentence.Should().BeNull();
        sentence1.NextSentence.Should().Be(sentence2);

        sentence2.PreviousSentence.Should().Be(sentence1);
        sentence2.NextSentence.Should().Be(sentence3);

        sentence2.PreviousSentence!.Text.Should().Be("aaa");
        sentence2.Text.Should().Be("bbb");
        sentence2.NextSentence!.Text.Should().Be("ccc");

        sentence3.PreviousSentence.Should().Be(sentence2);
        sentence3.NextSentence.Should().BeNull();
    }
}

