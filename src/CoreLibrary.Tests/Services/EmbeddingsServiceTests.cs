using CoreLibrary.Models;
using CoreLibrary.Services.Embeddings;
using CoreLibrary.Tests.Services.ObjectGenerativeFill.Tests;
using FluentAssertions;
using Microsoft.Extensions.Configuration;

namespace CoreLibrary.Tests.Services;

[TestClass]
public class EmbeddingsServiceTests
{
    [TestMethod]
    [TestCategory("RequiresGenerativeAi")]
    public async Task CreateEmbedding_ReturnsArrayOfNumbers()
    {
        // Arrange
        var sut = GetInstance();

        // Act
        var embedding = await sut.CreateEmbedding("test123");

        // Assert
        embedding.Should().NotBeNullOrEmpty();
        embedding.Count.Should().Be(1536, because: "it's an expected length of vector for text-embedding-3-small model");
    }

    [TestMethod]
    [TestCategory("RequiresGenerativeAi")]
    public async Task CreateEmbeddingRanTwice_ReturnsSameResult()
    {
        // Arrange
        var sut = GetInstance();

        // Act
        var embedding1 = await sut.CreateEmbedding("test");
        var embedding2 = await sut.CreateEmbedding("test");
        await sut.FlushCache();

        // Assert
        embedding1.Should().BeEquivalentTo(embedding2);
    }

    private static EmbeddingsService GetInstance()
    {
        // read API configuration
        var config = new ConfigurationBuilder().AddUserSecrets<GenerativeFillTests>().Build();
        var openAiDeveloperKey = config["OPENAI_DEVELOPER_KEY"];
        var openAiOrganizationId = config["OPENAI_ORGANIZATION_ID"];
        var azureOpenAiEndpoint = config["AZURE_OPENAI_ENDPOINT"];
        var azureOpenAiKey = config["AZURE_OPENAI_KEY"];

        // create settings model
        var cacheFilePath = Path.Combine("Resources", "embeddings-cache.mempack");
        var openAiCredentials = new OpenAiCredentials(azureOpenAiEndpoint, azureOpenAiKey, openAiOrganizationId, openAiDeveloperKey);
        var settings = new EmbeddingsServiceSettings(openAiCredentials, cacheFilePath);

        // create instance of the system under test
        var service = new EmbeddingsService(settings);
        return service;
    }
}
