using Azure.AI.OpenAI;
using CoreLibrary.Models;
using MathNet.Numerics.LinearAlgebra;
using Microsoft.Extensions.Logging;
using OpenAI;
using OpenAI.Embeddings;
using System.ClientModel;
using System.Collections.Concurrent;

namespace CoreLibrary.Services.Embeddings;

public class EmbeddingsService
{
    private readonly ILogger<EmbeddingsService> _logger;
    private readonly EmbeddingsCacheManager _embeddingsCacheManager;

    public EmbeddingsService(ILogger<EmbeddingsService> logger, EmbeddingsServiceSettings settings)
    {
        _logger = logger;
        _embeddingsCacheManager = new(settings.EmbeddingCacheFolder);

        const string model = "text-embedding-3-small";

        if (settings.OpenAiCredentials.BackendType == OpenAiBackend.Azure)
        {
            ApiKeyCredential credentials = new(settings.OpenAiCredentials.AzureOpenAiKey!);
            var azureOpenAiEndpointUrl = new Uri(settings.OpenAiCredentials.AzureOpenAiEndpoint!);
            AzureOpenAIClient azureClient = new(azureOpenAiEndpointUrl, credentials);
            _embeddingClient = azureClient.GetEmbeddingClient(model);
        }
        else
        {
            var openAIClientOptions = new OpenAIClientOptions() { OrganizationId = settings.OpenAiCredentials.OpenAiOrganizationId };
            var openAiCredentials = new ApiKeyCredential(settings.OpenAiCredentials.OpenAiDeveloperKey!);

            _embeddingClient = new(model, openAiCredentials, openAIClientOptions);
        }
    }
    private readonly EmbeddingClient _embeddingClient;

    public async Task<List<float>> CreateEmbedding(string inputText)
    {
        if (_embeddingsCacheManager.Cache.Cache.TryGetValue(inputText, out var embedding))
        {
            return embedding;
        }

        // Cache is primed, and it's unexpected if this breakpoint is hit on HP data - debug!
        var embeddingFromService = await CreateEmbeddingInternal([inputText]);
        var vector = embeddingFromService.Single();
        _embeddingsCacheManager.Cache.Cache.Add(inputText, vector);
        return vector;
    }

    public async Task PrimeEmbeddingsCache(IReadOnlyList<string> inputTexts)
    {
        var missingEmbeddings = inputTexts.Where(x => !_embeddingsCacheManager.Cache.Cache.ContainsKey(x)).Distinct();

        _logger.LogInformation($"{missingEmbeddings.Count()} embeddings missing, using OpenAI service to calculate them...");

        var chunks = missingEmbeddings.Chunk(30).ToList();
        foreach (var chunk in chunks)
        {
            _logger.LogInformation($"Calculating embeddings for chunk with {chunk.Count()} strings...");

            var texts = chunk.ToList();
            var embeddingFromService = await CreateEmbeddingInternal(texts);

            if (embeddingFromService.Count != texts.Count)
            {
                throw new InvalidOperationException($"Service responded with {embeddingFromService.Count} vectors, but we requested for {texts.Count}.");
            }

            for (var index = 0; index < embeddingFromService.Count; index++)
            {
                var text = texts[index];
                var embedding = embeddingFromService[index];
                _embeddingsCacheManager.Cache.Cache.Add(text, embedding);
            }

            _embeddingsCacheManager.FlushCache();
        }

    }

    public Task FlushCache()
    {
        _embeddingsCacheManager.FlushCache();
        return Task.CompletedTask;
    }

    private async Task<List<List<float>>> CreateEmbeddingInternal(List<string> inputTexts)
    {
        var embeddings = await _embeddingClient.GenerateEmbeddingsAsync(inputTexts);

        List<List<float>> embeddingsAsFloats = embeddings.Value
            .Select(x => x.ToFloats().ToArray().ToList())
            .ToList();
        return embeddingsAsFloats;
    }

    private readonly ConcurrentDictionary<List<float>, Vector<float>> _cache = new();

    // hot path
    // ideas:
    // 1) in-memory cache of Dense vectors - I iterate over words again and again
    // 2) computing in parallel
    public double CosineSimilarity(List<float> embedding1, List<float> embedding2)
    {
        if (!_cache.TryGetValue(embedding1, out var vector1))
        {
            vector1 = Vector<float>.Build.DenseOfEnumerable(embedding1);
            _cache.TryAdd(embedding1, vector1);
        }

        if (!_cache.TryGetValue(embedding2, out var vector2))
        {
            vector2 = Vector<float>.Build.DenseOfEnumerable(embedding2);
            _cache.TryAdd(embedding2, vector2);
        }

        return vector1.DotProduct(vector2) / (vector1.L2Norm() * vector2.L2Norm());
    }
}

public record EmbeddingsServiceSettings(
    OpenAiCredentials OpenAiCredentials,
    string EmbeddingCacheFolder);
