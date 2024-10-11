using CoreLibrary.Services.GenerativeAiClients;
using CoreLibrary.Utilities;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CoreLibrary.Services.ObjectGenerativeFill;

public record GenerativeFillSettings(string GenerativeFillCacheFolder);

/// <summary>
/// Helps process arrays of items instead of single item, to save on input tokens
/// </summary>
public class GenerativeFill(ILogger<GenerativeFill> logger, IGenerativeAiClient generativeAiClient, GenerativeFillSettings settings)
{
    // exposed for testing
    internal string GenerativeFillCacheFolder { get; } = settings.GenerativeFillCacheFolder;

    const string SystemChatMessage = "Your job is to transform array of input objects into array of output objects. Adhere to the JSON schema and use property descriptions for guidelines. Tha data will be used to teach language to students, so focus on the correctness of the generated translations and sentence examples.";

    /// <summary>
    ///  Excluded from DI so library client doesn't have to be aware of that internal class.
    /// </summary>
    private readonly GenerativeFillSchemaProvider _schemaProvider = new(settings.GenerativeFillCacheFolder);
    private readonly GenerativeFillCache _cache = new(settings.GenerativeFillCacheFolder);


    public async Task<T> FillMissingProperties<T>(string modelId, string modelClassId, T inputElement, int seed = 1) where T : ObjectWithId, new()
    {
        var inputElements = new List<T> { inputElement };
        var response = await FillMissingProperties(modelId, modelClassId, inputElements, seed);
        return response.Single();
    }

    public async Task<List<T>> FillMissingProperties<T>(string modelId, string modelClassId, List<T> inputItems, int seed = 1) where T : ObjectWithId, new()
    {
        // inputItems must have Id=null, otherwise they won't be filled.
        if (inputItems.Any(x => x.Id is not null))
            throw new ArgumentException("Input items must have Id=null to be filled.");

        // build prompt
        var promptTemplate = "Input contains array of items to process (in the `Items` property):\n" +
                               "\n" +
                               "```json\n" +
                               "{0}\n" +
                               "```\n" +
                               "\n" +
                               "The output should be in JSON and contain array of output items (with one output item for each input item, linked by Id).";

        var outputItems = _cache.FillFromCacheWherePossible(modelClassId, SystemChatMessage, promptTemplate, seed, inputItems);
        var itemsThatRequireApiCall = outputItems.Where(x => x.Id is null).ToList();

        // assign consecutive IDs to input elements
        for (var i = 0; i < outputItems.Count; i++)
            outputItems[i].Id = i + 1; // start from 1, just in case AI is trained to treat "0" differently

        if (itemsThatRequireApiCall.Count > 0)
        {
            logger.LogInformation("Filling missing properties for {NumItems} items", itemsThatRequireApiCall.Count);

            var schema = _schemaProvider.GenerateJsonSchemaForArrayOfItems<T>();

            // my unconfirmed hypothesis: the larger the batch, the more mistakes in the response.
            // e.g. in unit tests when I test 1-10 items in a batch, I observe no errors
            // but with batch of ~19 items, I see 40-50% items with validation errors. Maybe reducing the batch size will help?
            var chunks = itemsThatRequireApiCall.Chunk(10).ToList();
            var chunkNo = 0;

            foreach (var chunk in chunks)
            {
                var progressPercent = (chunkNo) * 100 / chunks.Count;
                logger.LogInformation("Processing chunk {ChunkNo} of {NumChunks}. {ProgressPercent}% done.", chunkNo, chunks.Count, progressPercent);

                var itemsThatRequireApiCallChunk = chunk.ToList();
                var inputSerialized = SerializeInput(itemsThatRequireApiCallChunk);
                var prompt = String.Format(promptTemplate, inputSerialized);
                var response = await generativeAiClient.GetAnswerToPrompt(modelId, modelClassId, SystemChatMessage, prompt, GenerativeAiClientResponseMode.StructuredOutput, seed, schema);
                var apiResultItems = DeserializeResponse<T>(response, itemsThatRequireApiCallChunk.Count);
                var newOutputItems = ReplacePlaceholdersWithFullObjects(outputItems, apiResultItems);

                var newItemsToStoreInCacheIds = apiResultItems.Select(x => x.Id).ToHashSet();
                var newItemsToStoreInCache = newOutputItems.Where(x => newItemsToStoreInCacheIds.Contains(x.Id)).ToList();
                _cache.SaveToCache(modelClassId, SystemChatMessage, promptTemplate, seed, newItemsToStoreInCache);

                outputItems = newOutputItems;
                chunkNo++;
            }

        }

        // return output
        return outputItems;
    }

    private static string SerializeInput<T>(List<T> inputObjects) where T : ObjectWithId, new()
    {
        var inputSerializationOptions = new JsonSerializerOptions();
        inputSerializationOptions.Converters.Add(new JsonStringEnumConverter());
        inputSerializationOptions.Converters.Add(new GenerativeFillSerializationConverter<T>(SerializationSetting.IdAndInputs));

        // wrapping array with an object because OpenAI API doesn't like JSON arrays as root element
        var inputArrayAsObject = new ArrayOfItemsWithIds<T>(inputObjects);
        var inputSerialized = JsonSerializer.Serialize(inputArrayAsObject, inputSerializationOptions);
        return inputSerialized;
    }

    private static List<T> DeserializeResponse<T>(string response, int numInputElements) where T : ObjectWithId, new()
    {
        var deserializationOptions = new JsonSerializerOptions();

        // gpt-4o-mini hallucinates on enum value despite Structured Output;
        // https://community.openai.com/t/structured-outputs-deep-dive/930169/40
        // a workaround is to use a custom JsonStringEnumConverterWithFallback: 
        deserializationOptions.Converters.Add(new JsonStringEnumConverterWithFallback());
        var resultObject = JsonSerializer.Deserialize<ArrayOfItemsWithIds<T>>(response, deserializationOptions);
        var resultItems = resultObject.Items;

        if (resultItems.Count != numInputElements)
        {
            throw new InvalidOperationException($"Number of items in response ({resultItems.Count}) doesn't match number of items in input ({numInputElements}).");
        }

        return resultItems;
    }

    private static List<T> ReplacePlaceholdersWithFullObjects<T>(List<T> partiallyFilledList, List<T> elementsFromApi) where T : ObjectWithId
    {
        var partiallyFilledListIncludingCurrentChunk = new List<T>();

        foreach (var element in partiallyFilledList)
        {
            // find element in API response
            var apiResult = elementsFromApi.SingleOrDefault(x => x.Id == element.Id);

            if (apiResult == null)
            {
                // object already read from cache earlier; rewrite to output
                partiallyFilledListIncludingCurrentChunk.Add(element);
            }
            else
            {
                // fill out the missing properties first
                CloneValuesOfPropertiesWithoutAttributes(element, apiResult);
                partiallyFilledListIncludingCurrentChunk.Add(apiResult);
            }
        }
        return partiallyFilledListIncludingCurrentChunk;
    }

    private static void CloneValuesOfPropertiesWithoutAttributes<T>(T element, T apiResult) where T : ObjectWithId
    {
        var properties = typeof(T).GetProperties();
        foreach (var prop in properties)
        {
            if (prop.Name == "Id")
                continue;

            if (!prop.CanWrite)
                continue;

            var aiFilledProperty = prop.GetCustomAttribute<FillWithAIAttribute>() is not null;
            if (!aiFilledProperty)
            {
                var propertyValueInApiResponseObject = prop.GetValue(element);
                prop.SetValue(apiResult, propertyValueInApiResponseObject);
            }
        }
    }
}
