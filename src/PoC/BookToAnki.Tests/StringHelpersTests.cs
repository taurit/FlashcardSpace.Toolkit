using BookToAnki.Services;
using FluentAssertions;

namespace BookToAnki.Tests;

[TestClass]
public class StringHelpersTests
{
    [DataRow("cat", "at", "ar", "car")]
    [DataRow("lalala", "la", "ma", "lalama")]
    [DataRow("lalala", "xx", "yy", "lalala")]
    [DataRow("l", "xx", "yy", "l")]
    [DataRow("", "xx", "yy", "")]
    [DataTestMethod]
    public void When_ReplaceEndingIsCalled_Expect_ProperResult(string input, string replaceEndingFrom,
        string replaceEndingTo, string expectedOutput)
    {
        // Ac
        var actualOutput = input.ReplaceEnding(replaceEndingFrom, replaceEndingTo);

        // Assert
        actualOutput.Should().Be(expectedOutput);
    }

    [DataRow("cat", "([ea])t", "$1r", "car")]
    [DataRow("cet", "([ea])t", "$1r", "cer")]
    [DataRow("Гаряча", "([чк])а", "$1ий", "Гарячий")]
    [DataRow("Гірка", "([чк])а", "$1ий", "Гіркий")]
    [DataTestMethod]
    public void When_ReplaceEndingRegexIsCalled_Expect_ProperResult(string input, string replaceEndingFrom,
        string replaceEndingTo, string expectedOutput)
    {
        // Ac
        var actualOutput = input.ReplaceEndingRegex(replaceEndingFrom, replaceEndingTo);

        // Assert
        actualOutput.Should().Be(expectedOutput);
    }


    [DataRow("data/chatGptExampleResponse01.txt")]
    [DataRow("data/chatGptExampleResponse02.txt")]
    [DataTestMethod]
    public void When_JsonIsExtractedFromChatGpt4Response_Expect_ProperResult(string inputFileName)
    {
        // Arrange
        var fileContent = File.ReadAllText(inputFileName);

        // Act
        var actualOutput = fileContent.GetJsonFromChatGptResponse();

        // Assert
        actualOutput.Should().Be(@"[
  {
    ""NominativeForm"":""книжка"",
    ""PolishTranslation"":""książka"",
    ""EnglishTranslation"":""book"",
    ""ExplanationInPolish"":""Obiekt związany z literaturą lub zapisem, składający się z kartek zebranych i połączonych razem"",
    ""ExplanationInEnglish"":""An item associated with literature or writing, consisting of collected and bound together pages""
  }
]");
    }
}
