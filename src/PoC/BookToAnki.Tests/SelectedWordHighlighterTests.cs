using BookToAnki.Services;
using FluentAssertions;

namespace BookToAnki.Tests;
[TestClass]
public class SelectedWordHighlighterTests
{
    [TestMethod]
    public void When_WordIsNotPresentInAHtmlSentence_Expect_NothingIsHighlighted()
    {
        // Arrange
        var sut = new SelectedWordHighlighter();

        // Act
        var highlighted = sut.HighlightWordInHtmlSentence("This text might contain HTML markup. Example: w<i>o</i>rd.", "other");

        // Assert
        highlighted.sentenceWithHighlight.Should().Be("This text might contain HTML markup. Example: w<i>o</i>rd.");
        highlighted.lastHightlightedWord.Should().Be(null);
    }

    [TestMethod]
    public void When_WordIsPresentInAHtmlSentence_Expect_ItIsHighlightedWithAStrongTag()
    {
        // Arrange
        var sut = new SelectedWordHighlighter();

        // Act
        var highlighted = sut.HighlightWordInHtmlSentence("This text might contain HTML markup. Example: w<i>o</i>rd.", "word");

        // Assert
        highlighted.sentenceWithHighlight.Should().Be("This text might contain HTML markup. Example: <strong>w<i>o</i>rd</strong>.");
        highlighted.lastHightlightedWord.Should().Be("w<i>o</i>rd");
    }

    [TestMethod]
    public void When_WordIsPresentMultipleTimesInAHtmlSentence_Expect_AllInstancesAreHighlighted()
    {
        // Arrange
        var sut = new SelectedWordHighlighter();

        // Act
        var highlighted = sut.HighlightWordInHtmlSentence("The word is present in this word sentence. Also another word here.", "word");

        // Assert
        highlighted.sentenceWithHighlight.Should().Be("The <strong>word</strong> is present in this <strong>word</strong> sentence. Also another <strong>word</strong> here.");
        highlighted.lastHightlightedWord.Should().Be("word");
    }

    [TestMethod]
    public void When_WordIsPartOfAnotherWordInAHtmlSentence_Expect_OnlyExactWordIsHighlighted()
    {
        // Arrange
        var sut = new SelectedWordHighlighter();

        // Act
        var highlighted = sut.HighlightWordInHtmlSentence("The word is present in this sword sentence. Also another sword here.", "word");

        // Assert
        highlighted.sentenceWithHighlight.Should().Be("The <strong>word</strong> is present in this sword sentence. Also another sword here.");
        highlighted.lastHightlightedWord.Should().Be("word");
    }

    [TestMethod]
    public void When_WordCaseDiffersInSentenceAndInSearchQuery_Expect_ItIsHighlightedAndCasingFromSentenceIsPreserved()
    {
        // Arrange
        var sut = new SelectedWordHighlighter();

        // Act
        var highlighted = sut.HighlightWordInHtmlSentence("Here's the word and WoRd.", "word");

        // Assert
        highlighted.sentenceWithHighlight.Should().Be("Here's the <strong>word</strong> and <strong>WoRd</strong>.");
        highlighted.lastHightlightedWord.Should().Be("WoRd");
    }

    [TestMethod]
    public void When_RealUseCaseIsProcessed_Expect_ProperHighlighting()
    {
        // Arrange
        var sut = new SelectedWordHighlighter();

        // Act
        var highlighted = sut.HighlightWordInHtmlSentence("Мелфой задов<span style=\"color:red\">о</span>лено р<span style=\"color:red\">у</span>шив до", "задоволено");

        // Assert
        highlighted.sentenceWithHighlight.Should().Be("Мелфой <strong>задов<span style=\"color:red\">о</span>лено</strong> р<span style=\"color:red\">у</span>шив до");
        highlighted.lastHightlightedWord.Should().Be("задов<span style=\"color:red\">о</span>лено");
    }

    [TestMethod]
    public void When_RealUseCaseIsProcessed_Expect_ProperHighlighting2()
    {
        // Arrange
        var sut = new SelectedWordHighlighter();

        // Act
        var highlighted = sut.HighlightWordInHtmlSentence("— З<span style=\"color:red\">а</span>вжд<span style=\"color:red\">и</span> мр<span style=\"color:red\">і</span>яла пот<span style=\"color:red\">и</span>снути вам р<span style=\"color:red\">у</span>ку…", "руку");

        // Assert
        highlighted.sentenceWithHighlight.Should().Be("— З<span style=\"color:red\">а</span>вжд<span style=\"color:red\">и</span> мр<span style=\"color:red\">і</span>яла пот<span style=\"color:red\">и</span>снути вам <strong>р<span style=\"color:red\">у</span>ку</strong>…");
        highlighted.lastHightlightedWord.Should().Be("р<span style=\"color:red\">у</span>ку");
    }

    [TestMethod]
    public void When_RealUseCaseIsProcessed_Expect_ProperHighlighting3()
    {
        // Arrange
        var sut = new SelectedWordHighlighter();

        // Act
        var highlighted = sut.HighlightWordInHtmlSentence("Гарр<span style=\"color:red\">і</span>, відійд<span style=\"color:red\">и</span>.", "Гаррі");

        // Assert
        highlighted.sentenceWithHighlight.Should().Be("<strong>Гарр<span style=\"color:red\">і</span></strong>, відійд<span style=\"color:red\">и</span>.");
        highlighted.lastHightlightedWord.Should().Be("Гарр<span style=\"color:red\">і</span>");
    }
}
