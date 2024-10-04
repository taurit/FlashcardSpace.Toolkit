using MathNet.Numerics.LinearAlgebra;
using OpenAI;
using OpenAI.Managers;
using OpenAI.ObjectModels.RequestModels;
using System.Collections.Concurrent;

namespace BookToAnki.Services.OpenAi;

public class EmbeddingsServiceWrapper
{
    private readonly EmbeddingsCacheManager _embeddingsCacheManager;

    public EmbeddingsServiceWrapper(string developerKey, string organization, EmbeddingsCacheManager embeddingsCacheManager)
    {
        _embeddingsCacheManager = embeddingsCacheManager;
        _openAiService = new(new OpenAiOptions
        {
            ApiKey = developerKey,
            Organization = organization
        });
    }
    private readonly OpenAIService _openAiService;

    public async Task<List<double>> CreateEmbedding(string inputText)
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
        var missingEmbeddings = inputTexts.Where(x => !_embeddingsCacheManager.Cache.Cache.ContainsKey(x));

        var chunks = missingEmbeddings.Chunk(30).ToList();
        foreach (var chunk in chunks)
        {
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

    private async Task<List<List<double>>> CreateEmbeddingInternal(List<string> inputTexts)
    {
        var embeddingResult = await _openAiService.Embeddings.CreateEmbedding(new EmbeddingCreateRequest
        {
            InputAsList = inputTexts,
            Model = OpenAI.ObjectModels.Models.TextEmbeddingV3Small,
        });

        if (embeddingResult.Successful)
        {
            return embeddingResult.Data.Select(x => x.Embedding).ToList();
        }

        if (embeddingResult.Error is not null)
        {
            throw new InvalidOperationException($"Generating embedding failed: {embeddingResult.Error.Code}: {embeddingResult.Error.Message}");
        }

        throw new InvalidOperationException("Generating embedding failed, but the service didn't provide any details");
    }

    private ConcurrentDictionary<List<double>, Vector<double>> _cache = new();

    // hot path
    // ideas:
    // 1) in-memory cache of Dense vectors - I iterate over words again and again
    // 2) computing in parallel
    public double CosineSimilarity(List<double> embedding1, List<double> embedding2)
    {
        if (!_cache.TryGetValue(embedding1, out var vector1))
        {
            vector1 = Vector<double>.Build.DenseOfEnumerable(embedding1);
            _cache.TryAdd(embedding1, vector1);
        }

        if (!_cache.TryGetValue(embedding2, out var vector2))
        {
            vector2 = Vector<double>.Build.DenseOfEnumerable(embedding2);
            _cache.TryAdd(embedding2, vector2);
        }

        return vector1.DotProduct(vector2) / (vector1.L2Norm() * vector2.L2Norm());
    }
}
