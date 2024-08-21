using MemoryPack;

namespace BookToAnki.Services.OpenAi;

[MemoryPackable]
public partial record EmbeddingsCache(Dictionary<string, List<double>> Cache);

public class EmbeddingsCacheManager
{
    private readonly string _cacheFilePath;

    public EmbeddingsCache Cache { get; }

    public EmbeddingsCacheManager(string cacheFilePath)
    {
        _cacheFilePath = cacheFilePath;

        if (File.Exists(_cacheFilePath))
        {
            var cacheContent = File.ReadAllBytes(cacheFilePath);
            var cached = MemoryPackSerializer.Deserialize<EmbeddingsCache>(cacheContent);

            Cache = cached ?? throw new FileLoadException($"Cache file `{cacheFilePath}` seems broken, remove it or restore from a backup.");
        }
        else
        {
            Cache = new EmbeddingsCache(new Dictionary<string, List<double>>());
        }
    }

    public void FlushCache()
    {
        var serializedData = MemoryPackSerializer.Serialize(Cache);
        File.WriteAllBytes(_cacheFilePath, serializedData);
    }

}
