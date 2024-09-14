using FluentAssertions;
using GenerateFlashcards.Tests.Infrastructure;
using System.Reflection;

namespace GenerateFlashcards.Tests.EndToEnd;

[TestClass]
public class ArgumentHandlingTests
{
    private static string GetPathToExecutable()
    {
        var currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        return Path.Combine(currentDirectory!, "GenerateFlashcards.exe");
    }

    [TestMethod]
    public async Task When_ApplicationIsRanWithoutAnyArguments_Expect_StatusCodeIsZeroAndHelpIsDisplayed()
    {
        // Arrange
        var executablePath = GetPathToExecutable();

        // Act
        var result = await ProcessRunner.Run(executablePath, arguments: "");

        // Assert
        result.StatusCode.Should().Be(0);
        result.ProcessOutput.Should().Contain("USAGE:");
    }

    [TestMethod]
    public async Task When_GenerateCommandIsRanWithoutAnyArguments_Expect_StatusCodeIsNonZeroAndErrorMessageIsDisplayed()
    {
        // Arrange
        var executablePath = GetPathToExecutable();

        // Act
        var result = await ProcessRunner.Run(executablePath, arguments: "generate-from-natural-language");

        // Assert
        result.StatusCode.Should().NotBe(0);
        result.ProcessOutput.Should().Contain("missing required argument");
    }

    [TestMethod]
    public async Task When_GenerateCommandIsRanWithUnsupportedInputLanguage_Expect_StatusCodeIsNonZeroAndErrorMessageIsDisplayed()
    {
        // Arrange
        var executablePath = GetPathToExecutable();

        // Act
        var result = await ProcessRunner.Run(executablePath,
            arguments: "generate-from-natural-language --inputLanguage klingon --outputLanguage English Resources/InputExample.Spanish.PlainText.txt");

        // Assert
        result.StatusCode.Should().NotBe(0);


        result.ProcessOutput.Should().Contain("klingon");
    }

    [TestMethod]
    public async Task When_GenerateCommandIsRanWithNonExistingInputFileParameter_Expect_StatusCodeIsNonZeroAndErrorMessageIsDisplayed()
    {
        // Arrange
        var executablePath = GetPathToExecutable();

        // Act
        var result = await ProcessRunner.Run(executablePath,
            arguments: "generate-from-natural-language --inputLanguage Spanish --outputLanguage English ThisFileDoesNotExist.txt");

        // Assert
        result.StatusCode.Should().NotBe(0);
        result.ProcessOutput.Should().Contain("ThisFileDoesNotExist.txt");
    }
}
