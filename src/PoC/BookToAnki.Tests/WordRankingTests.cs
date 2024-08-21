using BookToAnki.Services;
using FluentAssertions;

namespace BookToAnki.Tests;

[TestClass]
public class WordRankingTests
{
    [TestMethod]
    public void When_AskingForWordsInExactCasing_Expect_CorrectCount()
    {
        // Arrange
        var words = new string[] {
            "One",
            "Two"
        };
        WordRanking sut = new WordRanking(words);

        // Act, Assert
        sut.HowManyUsages("One").Should().Be(1);
        sut.HowManyUsages("Two").Should().Be(1);
    }

    [TestMethod]
    public void When_AskingForWordsNotInInput_Expect_AnswerIsZero()
    {
        // Arrange
        var words = new string[] {
            "One",
        };
        WordRanking sut = new WordRanking(words);

        // Act, Assert
        sut.HowManyUsages("Xxx").Should().Be(0);
        sut.HowManyUsages("").Should().Be(0);
        sut.HowManyUsages("OneOne").Should().Be(0);
        sut.HowManyUsages("On").Should().Be(0);
        sut.HowManyUsages("O").Should().Be(0);
    }

    [TestMethod]
    public void When_AskingForWordsInDifferentCasing_Expect_CorrectCount()
    {
        // Arrange
        var words = new string[] {
            "Two",
            "TWO",
        };
        WordRanking sut = new WordRanking(words);

        // Act, Assert
        sut.HowManyUsages("Two").Should().Be(2);
        sut.HowManyUsages("TWO").Should().Be(2);
        sut.HowManyUsages("two").Should().Be(2);
        sut.HowManyUsages("TwO").Should().Be(2);
        sut.HowManyUsages("Zero").Should().Be(0);
    }

    public record A(string Word)
    {
        public string Get() => Word;
    }
    public class B(string word)
    {
        public string Get() => word;
    }

    [TestMethod]
    public void Test()
    {
        Console.WriteLine(new A("asd").Word);
        Console.WriteLine(new B("asd").Get());
    }
}

