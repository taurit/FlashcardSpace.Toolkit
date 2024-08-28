using CoreLibrary.Utilities;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace CoreLibrary.Services.ObjectGenerativeFill;
internal class GenerativeFillCache
{
    private readonly string _rootFolder;

    public GenerativeFillCache(string rootFolder)
    {
        _rootFolder = rootFolder;
        Directory.CreateDirectory(rootFolder);
    }

    public void SaveToCache<T>(
            string modelClassId,
            string systemChatMessage,
            string promptTemplate,
            List<T> itemsToSaveInCache
        ) where T : ObjectWithId, new()
    {
        foreach (var item in itemsToSaveInCache)
        {
            var itemKeyValue = GetPrimaryKeyValue(item);

            var cacheFileName = $"{modelClassId}_" +
                                $"{item.GetType().Name}_" +
                                $"s{systemChatMessage.GetHashCodeStable(3)}_" +
                                $"p{promptTemplate.GetHashCodeStable(3)}_" +
                                $"k{itemKeyValue.GetHashCodeStable(3)}_" +
                                $"{itemKeyValue.GetFilenameFriendlyString()}" +
                                $".json" // Generative fill uses Structured Outputs and response is always JSON
                                ;

            var cacheFilePath = Path.Combine(_rootFolder, cacheFileName);

            if (File.Exists(cacheFilePath))
                continue;

            var serializedItem = JsonConvert.SerializeObject(item, Formatting.Indented);
            File.WriteAllText(cacheFilePath, serializedItem);
        }
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

        var inputProperties = properties.Where(p => p.GetCustomAttribute<FillWithAIAttribute>() == null && p.Name != "Id").ToList();
        if (inputProperties.Count == 1)
            return inputProperties[0].GetValue(item)?.ToString() ?? "";

        throw new InvalidOperationException($"Cannot determine primary key for object of type {typeof(T).Name}. Designate one using the [Key] attribute.");
    }
}
