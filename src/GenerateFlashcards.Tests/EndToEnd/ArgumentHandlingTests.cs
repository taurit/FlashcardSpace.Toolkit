using FluentAssertions;
using GenerateFlashcards.Tests.Infrastructure;
using System.Reflection;

namespace GenerateFlashcards.Tests.EndToEnd;

[TestClass]
public class ArgumentHandlingTests
{
    private string GetPathToExecutable()
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
        result.StandardOutput.Should().Contain("USAGE:");
        result.StandardError.Should().BeNullOrEmpty();
    }

    [TestMethod]
    public async Task When_GenerateCommandIsRanWithoutAnyArguments_Expect_StatusCodeIsNonZeroAndErrorMessageIsDisplayed()
    {
        // Arrange
        var executablePath = GetPathToExecutable();

        // Act
        var result = await ProcessRunner.Run(executablePath, arguments: "generate");

        // Assert
        result.StatusCode.Should().NotBe(0);
        result.StandardOutput.Should().Contain("missing required argument");
        result.StandardError.Should().BeNullOrEmpty();
    }

    [TestMethod]
    public async Task When_GenerateCommandIsRanWithUnsupportedInputLanguage_Expect_StatusCodeIsNonZeroAndErrorMessageIsDisplayed()
    {
        // Arrange
        var executablePath = GetPathToExecutable();

        // Act
        var result = await ProcessRunner.Run(executablePath, arguments: "generate --inputLanguage klingon --outputLanguage English input.txt");

        // Assert
        result.StatusCode.Should().NotBe(0);
        result.StandardOutput.Should().Contain("klingon");
        result.StandardError.Should().BeNullOrEmpty();
    }
}
