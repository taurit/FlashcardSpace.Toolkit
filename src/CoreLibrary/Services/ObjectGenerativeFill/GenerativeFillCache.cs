using CoreLibrary.Utilities;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace CoreLibrary.Services.ObjectGenerativeFill;
internal class GenerativeFillCache
{
    private readonly string _rootFolder;
    private readonly JsonSerializerOptions _serializationOptions;

    public GenerativeFillCache(string rootFolder)
    {
        _rootFolder = rootFolder;
        Directory.CreateDirectory(rootFolder);

        _serializationOptions = new JsonSerializerOptions();
        _serializationOptions.Converters.Add(new JsonStringEnumConverter());
        _serializationOptions.WriteIndented = true; // for convenience in debugging only

    }

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
            var itemKeyValue = GetPrimaryKeyValue(item);
            string itemTypeName = item.GetType().Name;
            var cacheFilePath = GenerateCacheFilePath<T>(modelClassId, systemChatMessage, promptTemplate, itemTypeName, itemKeyValue, seed);

            if (File.Exists(cacheFilePath))
                continue;

            var serializedItem = JsonSerializer.Serialize(item, _serializationOptions);
            File.WriteAllText(cacheFilePath, serializedItem);
        }
    }

    public T? ReadFromCache<T>(
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
                            $"{itemKeyValue.GetFilenameFriendlyString()}-" +
                            $"{modelClassId}-" +
                            $"s{systemChatMessage.GetHashCodeStable(3)}-" +

                            // explicit prompt needs to be in a cache key
                            $"p{promptTemplate.GetHashCodeStable(3)}-" +
                            // but requested JSON Schema of a response also contains prompt-like hints, and needs to be included
                            // so cache is invalidated when it changes
                            $"f{typeFingerprint.GetHashCodeStable(3)}-" +

                            $"k{itemKeyValue.GetHashCodeStable(3)}-" +
                            $"r{seed}" +

                            $".json" // Generative fill uses Structured Outputs and response is always JSON
            ;

        var cacheFilePath = Path.Combine(_rootFolder, cacheFileName);
        return cacheFilePath;
    }

    /// <summary>
    /// Returns value of primary key in the object.
    /// The primary key is selected with the following algorithm:
    /// 1) If there's only 1 property not marked with [FillWithAI] attribute (other than auto-generated Id), it's the primary key.
    /// 2) If there are multiple properties not marked with [FillWithAI] attribute, one must be marked with the [Key] attribute.
    /// </summary>
    private string GetPrimaryKeyValue<T>(T item) where T : ObjectWithId, new()
    {
        var properties = typeof(T).GetProperties();
        var primaryKeyProperty = properties.FirstOrDefault(p => p.GetCustomAttribute<KeyAttribute>() != null);
        if (primaryKeyProperty != null)
            return primaryKeyProperty.GetValue(item)?.ToString() ?? "";

        var inputProperties = properties.Where(p => p.GetCustomAttribute<FillWithAIAttribute>() == null && p.Name != "Id" && p.CanWrite).ToList();
        if (inputProperties.Count == 1)
            return inputProperties[0].GetValue(item)?.ToString() ?? "";

        throw new InvalidOperationException($"Cannot determine primary key for object of type {typeof(T).Name}. Designate one using the [Key] attribute.");
    }

    public List<T> FillFromCacheWherePossible<T>(string modelClassId, string systemChatMessage, string promptTemplate, int seed, List<T> inputItems)
        where T : ObjectWithId, new()
    {
        var outputItems = new List<T>();
        foreach (var objectToFill in inputItems)
        {
            var primaryKeyValue = GetPrimaryKeyValue(objectToFill);
            var cachedObject = ReadFromCache<T>(modelClassId, systemChatMessage, promptTemplate, seed, primaryKeyValue);

            outputItems.Add(cachedObject ?? objectToFill);
        }

        return outputItems;
    }
}
