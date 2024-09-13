using CoreLibrary.Services.GenerativeAiClients;
using FluentAssertions;

namespace CoreLibrary.Tests.Services;

[TestClass]
[TestCategory("RequiresGenerativeAi")]
public class GoogleGeminiClientTests
{
    [TestMethod]
    public async Task GetAnswerToPrompt_WhenAskedForCapitalOfFrance_ReturnsParis()
    {
        // Arrange
        var sut = GoogleGeminiClientFactory.CreateInstance();

        // Act
        var response = await sut.GetAnswerToPrompt("gemini-1.5-flash", "gemini-flash",
            "You are a helpful assistant",
            "What is the capital of France?",
            GenerativeAiClientResponseMode.PlainText);

        // Assert
        response.Should().Contain("Paris");

    }
}
