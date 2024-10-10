using CoreLibrary.Utilities;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace CoreLibrary.Services.ObjectGenerativeFill;
internal class GenerativeFillCache(string rootFolder)
{
    private readonly JsonSerializerOptions _serializationOptions = new()
    {
        WriteIndented = true, // needed for convenience in debugging only
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly string _rootFolder = rootFolder;

    public void SaveToCache<T>(
            string modelClassId,
            string systemChatMessage,
            string promptTemplate,
            int seed,
            List<T> itemsToSaveInCache
        ) where T : ObjectWithId, new()
    {
        foreach (var item in itemsToSaveInCache)
        {
            var compoundKeyFingerprint = GetCompoundKeyFingerprint(item);
            string itemTypeName = item.GetType().Name;
            var cacheFilePath = GenerateCacheFilePath<T>(modelClassId, systemChatMessage, promptTemplate, itemTypeName, compoundKeyFingerprint, seed);

            if (File.Exists(cacheFilePath))
                continue;

            var serializedItem = JsonSerializer.Serialize(item, _serializationOptions);
            File.WriteAllText(cacheFilePath, serializedItem);
        }
    }

    private T? ReadFromCache<T>(
        string modelClassId,
        string systemChatMessage,
        string promptTemplate,
        int seed,
        string itemKeyValue
    ) where T : ObjectWithId, new()
    {
        var itemTypeName = typeof(T).Name;
        var cacheFilePath = GenerateCacheFilePath<T>(modelClassId, systemChatMessage, promptTemplate, itemTypeName, itemKeyValue, seed);

        if (!File.Exists(cacheFilePath))
            return null;

        var serializedItem = File.ReadAllText(cacheFilePath);
        var deserializedItem = JsonSerializer.Deserialize<T>(serializedItem, _serializationOptions);
        if (deserializedItem == null)
            throw new InvalidOperationException($"Failed to deserialize item from cache file {cacheFilePath}. It's unexpected, debug!");

        deserializedItem.Id = 0;

        return deserializedItem;
    }

    private string GenerateCacheFilePath<T>(string modelClassId, string systemChatMessage, string promptTemplate, string itemTypeName,
        string itemKeyValue, int seed)
    {
        var typeFingerprint = ClassFingerprintProvider.GenerateTypeFingerprint(typeof(T));

        var cacheFileName = $"{itemTypeName}-" +
                            $"{modelClassId}-" +
                            $"s{systemChatMessage.GetHashCodeStable(3)}-" +

                            // explicit prompt needs to be in a cache key
                            $"p{promptTemplate.GetHashCodeStable(3)}-" +
                            // but requested JSON Schema of a response also contains prompt-like hints, and needs to be included
                            // so cache is invalidated when it changes
                            $"f{typeFingerprint.GetHashCodeStable(3)}-" +

                            $"k{itemKeyValue.GetHashCodeStable(5)}-" +
                            $"r{seed}" +

                            $".json" // Generative fill uses Structured Outputs and response is always JSON
            ;

        var cacheFilePath = Path.Combine(_rootFolder, cacheFileName);
        _rootFolder.EnsureDirectoryExists();

        return cacheFilePath;
    }

    /// <summary>
    /// Returns a fingerprint of the set of all *input* properties in the object.
    /// </summary>
    /// <returns>An string of arbitrary length and format that should differ if any input properties differs.</returns>
    private string GetCompoundKeyFingerprint<T>(T item) where T : ObjectWithId, new()
    {
        var properties = typeof(T).GetProperties();

        var inputProperties = properties.Where(p => p.GetCustomAttribute<FillWithAIAttribute>() == null && p.Name != "Id" && p.CanWrite).ToList();
        if (inputProperties.Count == 0)
            throw new InvalidOperationException($"Cannot get key fingerprint for object of type {typeof(T).Name}. Forgot to add at least 1 input property to the type?");

        var inputPropertiesValues = inputProperties.Select(x => x.GetValue(item)?.ToString() ?? "");
        const string? separatorUnlikelyToBeFoundInPropertyValues = "###";

        var fingerprint = string.Join(separatorUnlikelyToBeFoundInPropertyValues, inputPropertiesValues);
        return fingerprint;
    }

    public List<T> FillFromCacheWherePossible<T>(string modelClassId, string systemChatMessage, string promptTemplate, int seed, List<T> inputItems)
        where T : ObjectWithId, new()
    {
        var outputItems = new List<T>();
        foreach (var objectToFill in inputItems)
        {
            var compoundKeyFingerprint = GetCompoundKeyFingerprint(objectToFill);
            var cachedObject = ReadFromCache<T>(modelClassId, systemChatMessage, promptTemplate, seed, compoundKeyFingerprint);

            outputItems.Add(cachedObject ?? objectToFill);
        }

        return outputItems;
    }
}
