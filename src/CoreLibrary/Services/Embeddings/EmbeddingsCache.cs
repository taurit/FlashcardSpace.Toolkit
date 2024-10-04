using CoreLibrary.Utilities;
using MemoryPack;

namespace CoreLibrary.Services.Embeddings;

[MemoryPackable]
public partial record EmbeddingsCache(Dictionary<string, List<float>> Cache);

public class EmbeddingsCacheManager
{
    private readonly string _cacheFilePath;

    public EmbeddingsCache Cache { get; }

    public EmbeddingsCacheManager(string cacheFolder)
    {
        cacheFolder.EnsureDirectoryExists();
        _cacheFilePath = Path.Combine(cacheFolder, "embeddings-cache.mempack");

        if (File.Exists(_cacheFilePath))
        {
            var cacheContent = File.ReadAllBytes(_cacheFilePath);
            var cached = MemoryPackSerializer.Deserialize<EmbeddingsCache>(cacheContent);

            Cache = cached ?? throw new FileLoadException($"Cache file `{_cacheFilePath}` seems broken, remove it or restore from a backup.");
        }
        else
        {
            Cache = new EmbeddingsCache(new Dictionary<string, List<float>>());
        }
    }

    public void FlushCache()
    {
        var serializedData = MemoryPackSerializer.Serialize(Cache);
        File.WriteAllBytes(_cacheFilePath, serializedData);
    }

}
