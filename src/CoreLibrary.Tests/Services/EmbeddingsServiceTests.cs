using CoreLibrary.Models;
using CoreLibrary.Services.Embeddings;
using CoreLibrary.Tests.Services.ObjectGenerativeFill.Tests;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

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

    [TestMethod]
    [TestCategory("RequiresGenerativeAi")]
    public async Task CreateEmbeddingRanTwiceWithDifferentText_ReturnsDifferentResult()
    {
        // Arrange
        var sut = GetInstance();

        // Act
        var embedding1 = await sut.CreateEmbedding("test1");
        var embedding2 = await sut.CreateEmbedding("test2");
        await sut.FlushCache();

        // Assert
        embedding1.Should().NotBeEquivalentTo(embedding2);
    }

    [TestMethod]
    [TestCategory("RequiresGenerativeAi")]
    public async Task EmbeddingsForSimilarText_ReturnsHigherScoreThanForDissimilarText()
    {
        // Arrange
        var sut = GetInstance();

        var similarTextPairs = new List<Tuple<string, string>>()
        {
            new("dog", "puppy"),
            new("cat", "kitten"),
            new("car", "vehicle"),
            new("house", "building"),
            new("apple", "fruit"),
            new("banana", "fruit"),
        };

        var dissimilarTextPairs = new List<Tuple<string, string>>()
        {
            new("dog", "car"),
            new("dog", "house"),
            new("dog", "apple"),
            new("dog", "banana"),
            new("dog", "bone")
        };

        var similarColumn1 = similarTextPairs.Select(x => x.Item1).ToList().AsReadOnly();
        var similarColumn2 = similarTextPairs.Select(x => x.Item2).ToList().AsReadOnly();
        var dissimilarColumn1 = dissimilarTextPairs.Select(x => x.Item1).ToList().AsReadOnly();
        var dissimilarColumn2 = dissimilarTextPairs.Select(x => x.Item2).ToList().AsReadOnly();

        // Act
        await sut.PrimeEmbeddingsCache(similarColumn1);
        await sut.PrimeEmbeddingsCache(similarColumn2);
        await sut.PrimeEmbeddingsCache(dissimilarColumn1);
        await sut.PrimeEmbeddingsCache(dissimilarColumn2);
        await sut.FlushCache();

        // Act
        var similarityScoresForSimilarWords = new List<double>();
        foreach (var (text1, text2) in similarTextPairs)
        {
            var embedding1 = await sut.CreateEmbedding(text1);
            var embedding2 = await sut.CreateEmbedding(text2);
            var similarity = sut.CosineSimilarity(embedding1, embedding2);
            Console.WriteLine($"For {text1} and {text2} similarity is {similarity}");
            similarityScoresForSimilarWords.Add(similarity);
        }

        Console.WriteLine("");

        var similarityScoresForDissimilarWords = new List<double>();
        foreach (var (text1, text2) in dissimilarTextPairs)
        {
            var embedding1 = await sut.CreateEmbedding(text1);
            var embedding2 = await sut.CreateEmbedding(text2);
            var similarity = sut.CosineSimilarity(embedding1, embedding2);
            Console.WriteLine($"For {text1} and {text2} similarity is {similarity}");
            similarityScoresForDissimilarWords.Add(similarity);
        }

        // Assert
        var minScoreSimilar = similarityScoresForSimilarWords.Min();
        var maxScoreDissimilar = similarityScoresForDissimilarWords.Max();

        Console.WriteLine("");
        Console.WriteLine($"Min score for similar words: {minScoreSimilar}");
        Console.WriteLine($"Max score for dissimilar words: {maxScoreDissimilar}");

        minScoreSimilar.Should().BeGreaterThan(maxScoreDissimilar);
        var exampleThresholdThatWorks = maxScoreDissimilar + (minScoreSimilar - maxScoreDissimilar) / 2;
        Console.WriteLine($"The threshold allowing to separate similar and dissimilar words is somewhere between {maxScoreDissimilar} and {minScoreSimilar}, e.g., {exampleThresholdThatWorks}");
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

        var logger = LoggerFactory.Create(builder => builder.AddConsole())
                        .CreateLogger<EmbeddingsService>();

        // create instance of the system under test
        var service = new EmbeddingsService(logger, settings);
        return service;
    }
}
