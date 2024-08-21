using AdvancedSentenceExtractor.Services;
using BookToAnki.Services;
using FluentAssertions;

namespace BookToAnki.Tests;

[TestClass]
public class WordTokenizerTests
{
    [TestMethod]
    public void When_SentenceContainsNoWords_Expect_ListOfWordsIsEmpty()
    {
        // Arrange
        var sut = new WordTokenizer();

        // Act
        var words = sut.GetWords("");

        // Assert
        CollectionAssert.AreEqual(new List<string> { }, words);
    }

    [TestMethod]
    public void When_SentenceContainsOneWord_Expect_ListOfWordsHasOneElement()
    {
        // Arrange
        var sut = new WordTokenizer();

        // Act
        var words = sut.GetWords("Hello");

        // Assert
        CollectionAssert.AreEqual(new List<string>() { "Hello" }, words);
    }

    [TestMethod]
    public void When_SentenceContainsMultipleWords_Expect_AllOfThemAreReturned()
    {
        // Arrange
        var sut = new WordTokenizer();

        // Act
        var words = sut.GetWords("‘Comb your hair!’ he barked");

        // Assert
        CollectionAssert.AreEqual(new List<string>() { "Comb", "your", "hair", "he", "barked" }, words);
    }

    [TestMethod]
    public void When_SentenceContainsPunctuation_Expect_PunctuationIsNotIncludedInWords()
    {
        // Arrange
        var sut = new WordTokenizer();

        // Act
        var words = sut.GetWords("Hello, world!");

        // Assert
        CollectionAssert.AreEqual(new List<string>() { "Hello", "world" }, words);
    }

    [TestMethod]
    public void When_SentenceContainsNumbers_Expect_NumbersAreRetained()
    {
        // Arrange
        var sut = new WordTokenizer();

        // Act
        var words = sut.GetWords("I have 2 cats");

        // Assert
        CollectionAssert.AreEqual(new List<string>() { "I", "have", "2", "cats" }, words);
    }

    [TestMethod]
    public void When_SentenceContainsHyphenatedWords_Expect_HyphenatedWordsAreSeparated()
    {
        // Arrange
        var sut = new WordTokenizer();

        // Act
        var words = sut.GetWords("Mother-in-law and son-in-law were arguing");

        // Assert
        CollectionAssert.AreEqual(new List<string>() { "Mother", "in", "law", "and", "son", "in", "law", "were", "arguing" }, words);
    }

    [TestMethod]
    public void When_SentenceContainsHyphenatedWordsInUkrainian_Expect_HyphenatedWordsAreSeparated()
    {
        // Arrange
        var sut = new WordTokenizer();

        // Act
        var words = sut.GetWords("тільки-но");

        // Assert
        CollectionAssert.AreEqual(new List<string>() { "тільки", "но" }, words);
    }

    [TestMethod]
    public void When_SentenceContainsApostrophes_Expect_WordsAreSplitCorrectly()
    {
        // Arrange
        var sut = new WordTokenizer();

        // Act
        var words = sut.GetWords("I can't believe it's not butter");

        // Assert
        CollectionAssert.AreEqual(new List<string>() { "I", "can't", "believe", "it's", "not", "butter" }, words);
    }

    [TestMethod]
    public void When_SentenceContainsApostrophesUsingLessCommonCharacter_Expect_WordsAreSplitCorrectly()
    {
        // Arrange
        var sut = new WordTokenizer();

        // Act
        var words = sut.GetWords("She can’t take him.");

        // Assert
        CollectionAssert.AreEqual(new List<string>() { "She", "can't", "take", "him" }, words);
    }

    [DataTestMethod]
    [DataRow("can't", "can't")]
    [DataRow("can’t", "can't")]
    [DataRow("he’d", "he'd")]
    [DataRow("don't", "don't")]
    [DataRow("Dumbledore'owi", "Dumbledore'owi")]
    [DataRow("Harry’emu", "Harry'emu")]
    [DataRow("Snape'em", "Snape'em")]
    [DataRow("пов'язані", "пов'язані")]
    [DataRow("дев'яту", "дев'яту")]
    [DataRow("п'ятьох", "п'ятьох")]
    [DataRow("won't", "won't")]
    [DataRow("it’s", "it's")]
    [DataRow("she’ll", "she'll")]
    [DataRow("they’ve", "they've")]
    [DataRow("we’re", "we're")]
    [DataRow("o’clock", "o'clock")]
    [DataRow("y'all", "y'all")]
    [DataRow("ma’am", "ma'am")]
    [DataRow("ne'er", "ne'er")]
    [DataRow("how’d", "how'd")]
    [DataRow("м'ясо", "м'ясо")]
    [DataRow("п'є", "п'є")]
    [DataRow("тр'єхгодинний", "тр'єхгодинний")]
    [DataRow("сім’я", "сім'я")]
    [DataRow("між’ю", "між'ю")]
    [DataRow("кінь’ом", "кінь'ом")]
    [DataRow("лев’яті", "лев'яті")]
    public void When_SentenceContainsApostrophesInCommonApostrophedWords_Expect_WordsAreSplitCorrectlyAndApostrophesAreNormalized(string wordWithApostrophe, string wordWithApostropheNormalized)
    {
        // Arrange
        var sut = new WordTokenizer();

        // Act
        var words = sut.GetWords($"WordOne {wordWithApostrophe} another.");

        // Assert
        CollectionAssert.AreEqual(new List<string>() { "WordOne", wordWithApostropheNormalized, "another" }, words);
    }

    [TestMethod]
    public void When_SentenceContainsApostrophesOutsideOfWords_Expect_TheyAreIgnored()
    {
        // Arrange
        var sut = new WordTokenizer();

        // Act
        var words = sut.GetWords($"‘Just leave me here,’ he said");

        // Assert
        CollectionAssert.AreEqual(new List<string>() { "Just", "leave", "me", "here", "he", "said" }, words);
    }

    [TestMethod]
    public void When_SentenceContainsCompoundWords_Expect_CompoundWordsAreKeptTogether()
    {
        // Arrange
        var sut = new WordTokenizer();

        // Act
        var words = sut.GetWords("Blackbird and blowfish were playing");

        // Assert
        CollectionAssert.AreEqual(new List<string>() { "Blackbird", "and", "blowfish", "were", "playing" }, words);
    }

    [TestMethod]
    public void When_SentenceContainsDiacritics_Expect_DiacriticsAreKeptInWords()
    {
        // Arrange
        var sut = new WordTokenizer();

        // Act
        var words = sut.GetWords("Zażółć gęślą jaźń");  // Polish sentence

        // Assert
        CollectionAssert.AreEqual(new List<string>() { "Zażółć", "gęślą", "jaźń" }, words);
    }

    [TestMethod]
    public void When_SentenceContainsCyrillicLetters_Expect_WordsAreSplitCorrectly()
    {
        // Arrange
        var sut = new WordTokenizer();

        // Act
        var words = sut.GetWords("Він має дві кішки");  // Ukrainian sentence: "He has two cats"

        // Assert
        CollectionAssert.AreEqual(new List<string>() { "Він", "має", "дві", "кішки" }, words);
    }

    [TestMethod]
    public void When_SentenceEndsWithThreeDots_Expect_ProperSplit()
    {
        // Arrange
        var sut = new WordTokenizer();

        // Act
        var words = sut.GetWords("— Ви хочете сказати, що він був причетний до...");

        // Assert
        words.Should().BeEquivalentTo(new List<string>() { "Ви", "хочете", "сказати", "що", "він", "був", "причетний", "до" });
    }

    [TestMethod]
    public void When_SentenceContainsDot_Expect_NoWordContainsDot()
    {
        // Arrange
        var sut = new WordTokenizer();

        // Act
        var words = sut.GetWords("— Вони не винні, — лагідно сказав Дамблдор. — Цілих ");

        // Assert
        CollectionAssert.AreEqual(new List<string>() { "Вони", "не", "винні", "лагідно", "сказав", "Дамблдор", "Цілих" }, words);
    }

    [DataTestMethod]
    // English
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
    // Polish
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
    // Ukrainian
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
    public void When_InputContainsCommonAbbreviations_Expect_DotsAreNotMistakenForWordBorders(string abbreviation)
    {
        // Arrange
        var sut = new WordTokenizer();

        // Act
        var words = sut.GetWords($"Word1 {abbreviation} word2");

        // Assert
        CollectionAssert.AreEqual(new List<string>() { "Word1", abbreviation, "word2" }, words);
    }
}
