using AdvancedSentenceExtractor.Services;
using BookToAnki.Services;
using Moq;

namespace BookToAnki.Tests;

[TestClass]
public class SentenceTokenizerTests
{
    [TestMethod]
    public void When_ThereAreTwoSentencesInAString_Expect_TwoSentencesInOutput()
    {
        // Arrange
        var wordTokenizerMock = new Mock<IWordTokenizer>(MockBehavior.Strict);
        wordTokenizerMock.Setup(x => x.GetWords(It.IsAny<string>())).Returns(new List<string>() { "Irrelevant to this test." });
        var sentenceFactory = new SentenceFactory(wordTokenizerMock.Object);

        var sut = new SentenceTokenizer(sentenceFactory);

        // Act
        var sentences = sut.TokenizeBook("Ala ma kota. Kot ma Alę.");

        // Assert
        Assert.AreEqual(2, sentences.Count);
        CollectionAssert.AreEqual(new List<string> { "Ala ma kota.", "Kot ma Alę." }, sentences.Select(x => x.Text).ToList());
    }

    [TestMethod]
    public void When_ThereAreNoSentencesInAString_Expect_EmptyList()
    {
        // Arrange
        var wordTokenizerMock = new Mock<IWordTokenizer>(MockBehavior.Strict);
        wordTokenizerMock.Setup(x => x.GetWords(It.IsAny<string>())).Returns(new List<string>() { "Irrelevant to this test." });
        var sentenceFactory = new SentenceFactory(wordTokenizerMock.Object);

        var sut = new SentenceTokenizer(sentenceFactory);

        // Act
        var sentences = sut.TokenizeBook("");

        // Assert
        Assert.AreEqual(0, sentences.Count);
    }

    [TestMethod]
    public void When_SentenceEndsWithQuestionMark_Expect_ItIsRecognizedAsASentence()
    {
        // Arrange
        var wordTokenizerMock = new Mock<IWordTokenizer>(MockBehavior.Strict);
        wordTokenizerMock.Setup(x => x.GetWords(It.IsAny<string>())).Returns(new List<string>() { "Irrelevant to this test." });
        var sentenceFactory = new SentenceFactory(wordTokenizerMock.Object);

        var sut = new SentenceTokenizer(sentenceFactory);

        // Act
        var sentences = sut.TokenizeBook("Is this a question? Yes, this is a question.");

        // Assert
        Assert.AreEqual(2, sentences.Count);
        CollectionAssert.AreEqual(new List<string> { "Is this a question?", "Yes, this is a question." }, sentences.Select(x => x.Text).ToList());
    }

    [TestMethod]
    public void When_SentenceEndsWithExclamationMark_Expect_ItIsRecognizedAsASentence()
    {
        // Arrange
        var wordTokenizerMock = new Mock<IWordTokenizer>(MockBehavior.Strict);
        wordTokenizerMock.Setup(x => x.GetWords(It.IsAny<string>())).Returns(new List<string>() { "Irrelevant to this test." });
        var sentenceFactory = new SentenceFactory(wordTokenizerMock.Object);

        var sut = new SentenceTokenizer(sentenceFactory);

        // Act
        var sentences = sut.TokenizeBook("This is an exclamation! Oh yes it is.");

        // Assert
        Assert.AreEqual(2, sentences.Count);
        CollectionAssert.AreEqual(new List<string> { "This is an exclamation!", "Oh yes it is." }, sentences.Select(x => x.Text).ToList());
    }

    [TestMethod]
    public void When_SentencesAreSeparatedByNewLines_Expect_SentencesAreSplitCorrectly()
    {
        // Arrange
        var wordTokenizerMock = new Mock<IWordTokenizer>(MockBehavior.Strict);
        wordTokenizerMock.Setup(x => x.GetWords(It.IsAny<string>())).Returns(new List<string>() { "Irrelevant to this test." });
        var sentenceFactory = new SentenceFactory(wordTokenizerMock.Object);

        var sut = new SentenceTokenizer(sentenceFactory);

        // Act
        var sentences = sut.TokenizeBook("First sentence.\nSecond sentence.");

        // Assert
        Assert.AreEqual(2, sentences.Count);
        CollectionAssert.AreEqual(new List<string> { "First sentence.", "Second sentence." }, sentences.Select(x => x.Text).ToList());
    }

    [TestMethod]
    public void When_SentencesContainDialogueLines_Expect_SentencesWithingDialogAreSplitToo()
    {
        // Arrange
        var wordTokenizerMock = new Mock<IWordTokenizer>(MockBehavior.Strict);
        wordTokenizerMock.Setup(x => x.GetWords(It.IsAny<string>())).Returns(new List<string>() { "Irrelevant to this test." });
        var sentenceFactory = new SentenceFactory(wordTokenizerMock.Object);

        var sut = new SentenceTokenizer(sentenceFactory);

        // Act
        var sentences = sut.TokenizeBook("‘Up! Get up! Now!’\n\nHarry woke with a start. His aunt rapped on the door again.");

        // Assert
        var expected = new List<string> { "Up!", "Get up!", "Now!", "Harry woke with a start.", "His aunt rapped on the door again." };
        var actual = sentences.Select(x => x.Text).ToList();
        CollectionAssert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void When_SentencesContainDialogueLines2_Expect_SentencesWithingDialogAreSplitToo()
    {
        // Arrange
        var wordTokenizer = new WordTokenizer();
        var sentenceFactory = new SentenceFactory(wordTokenizer);

        var sut = new SentenceTokenizer(sentenceFactory);

        // Act
        var sentences = sut.TokenizeBook("— Ви мені лестите, — мовив спокійно Дамблдор. — Волдеморт мав силу, якої я ніколи не матиму.");

        // Assert
        var expected = new List<string> { "— Ви мені лестите, — мовив спокійно Дамблдор.", "— Волдеморт мав силу, якої я ніколи не матиму." };
        var actual = sentences.Select(x => x.Text).ToList();
        CollectionAssert.AreEqual(expected, actual);
    }

    [DataTestMethod]
    [DataRow("Mr.")]
    [DataRow("Dr.")]
    [DataRow("Prof.")]
    [DataRow("Mrs.")]
    [DataRow("Ms.")]
    [DataRow("Jr.")]
    [DataRow("Sr.")]
    [DataRow("Inc.")]
    [DataRow("Co.")]
    [DataRow("St.")]
    [DataRow("Ave.")]
    [DataRow("U.S.")]
    [DataRow("U.K.")]
    [DataRow("E.g.")]
    [DataRow("I.e.")]
    public void When_InputContainsCommonEnglishAbbreviations_Expect_TheyAreNotMistakenForSentenceEnds(string abbreviation)
    {
        // Arrange
        var wordTokenizerMock = new Mock<IWordTokenizer>(MockBehavior.Strict);
        wordTokenizerMock.Setup(x => x.GetWords(It.IsAny<string>())).Returns(new List<string>() { "Irrelevant to this test." });
        var sentenceFactory = new SentenceFactory(wordTokenizerMock.Object);

        var sut = new SentenceTokenizer(sentenceFactory);

        // Act
        var sentences = sut.TokenizeBook($"{abbreviation} Smith went to Washington.");

        // Assert
        Assert.AreEqual(1, sentences.Count);
        Assert.AreEqual($"{abbreviation} Smith went to Washington.", sentences.First().Text);
    }

    [DataTestMethod]
    [DataRow("Np.")]
    [DataRow("Dr.")]
    [DataRow("Prof.")]
    [DataRow("Nr.")]
    [DataRow("Tzn.")]
    [DataRow("Gł.")]
    [DataRow("Ul.")]
    [DataRow("Mec.")]
    [DataRow("Ks.")]
    [DataRow("Pl.")]
    [DataRow("Ogł.")]
    [DataRow("Max.")]
    [DataRow("Min.")]
    [DataRow("C.k.")]
    [DataRow("św.")]
    [DataRow("Płk.")]
    [DataRow("Śp.")]
    [DataRow("Tj.")]
    [DataRow("Br.")]
    [DataRow("Poz.")]
    public void When_InputContainsCommonPolishAbbreviations_Expect_TheyAreNotMistakenForSentenceEnds(string abbreviation)
    {
        // Arrange
        var wordTokenizerMock = new Mock<IWordTokenizer>(MockBehavior.Strict);
        wordTokenizerMock.Setup(x => x.GetWords(It.IsAny<string>())).Returns(new List<string>() { "Irrelevant to this test." });
        var sentenceFactory = new SentenceFactory(wordTokenizerMock.Object);

        var sut = new SentenceTokenizer(sentenceFactory);

        // Act
        var sentences = sut.TokenizeBook($"Słowopierwsze {abbreviation} słowotrzecie.");

        // Assert
        Assert.AreEqual(1, sentences.Count);
        Assert.AreEqual($"Słowopierwsze {abbreviation} słowotrzecie.", sentences.First().Text);
    }


    [DataTestMethod]
    [DataRow("див.")]
    [DataRow("ст.")]
    [DataRow("н.е.")]
    [DataRow("тис.")]
    [DataRow("грн.")]
    [DataRow("т.д.")]
    [DataRow("обл.")]
    [DataRow("кв.")]
    [DataRow("с.г.")]
    [DataRow("млн.")]
    [DataRow("дн.")]
    [DataRow("р.")]
    [DataRow("просп.")]
    [DataRow("вул.")]
    [DataRow("буд.")]
    [DataRow("км.")]
    [DataRow("рр.")]
    [DataRow("зб.")]
    [DataRow("кн.")]
    public void When_InputContainsCommonUkrainianAbbreviations_Expect_TheyAreNotMistakenForSentenceEnds(string abbreviation)
    {
        // Arrange
        var wordTokenizerMock = new Mock<IWordTokenizer>(MockBehavior.Strict);
        wordTokenizerMock.Setup(x => x.GetWords(It.IsAny<string>())).Returns(new List<string>() { "Irrelevant to this test." });
        var sentenceFactory = new SentenceFactory(wordTokenizerMock.Object);

        var sut = new SentenceTokenizer(sentenceFactory);

        // Act
        var sentences = sut.TokenizeBook($"Słowopierwsze {abbreviation} słowotrzecie.");

        // Assert
        Assert.AreEqual(1, sentences.Count);
        Assert.AreEqual($"Słowopierwsze {abbreviation} słowotrzecie.", sentences.First().Text);
    }
}
