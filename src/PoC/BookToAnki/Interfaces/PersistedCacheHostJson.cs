using System.Text.Encodings.Web;
using System.Text.Json;

namespace BookToAnki.Interfaces;
public abstract class PersistedCacheHostJson<T> where T : new()
{
    private readonly string? _cacheFilePath;
    protected readonly T Cache;

    protected PersistedCacheHostJson(string? cacheFilePath)
    {
        _cacheFilePath = cacheFilePath;
        Cache = LoadCacheFromFile(_cacheFilePath);
    }

    private T LoadCacheFromFile(string? fileName)
    {
        if (fileName is null || !File.Exists(fileName)) return new T();

        try
        {
            var json = File.ReadAllText(fileName);
            var deserialized = JsonSerializer.Deserialize<T>(json);
            if (deserialized is null) return new T();
            return deserialized;
        }
        catch (Exception)
        {
            return new T();
        }
    }

    public void SaveCache()
    {
        if (_cacheFilePath is null) return;

        var options = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true
        };

        var json = JsonSerializer.Serialize(Cache, options);
        File.WriteAllText(_cacheFilePath, json);
    }
}
