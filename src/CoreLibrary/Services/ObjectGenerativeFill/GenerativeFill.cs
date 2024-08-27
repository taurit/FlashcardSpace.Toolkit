using CoreLibrary.Services.GenerativeAiClients;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CoreLibrary.Services.ObjectGenerativeFill;

/// <summary>
/// Helps process arrays of items instead of single item, to save on input tokens
/// </summary>
public class GenerativeFill(IGenerativeAiClient generativeAiClient)
{
    const string SystemChatMessage = "You are a helpful assistant";

    /// <summary>
    ///  Excluded from DI so library client doesn't have to be aware of that internal class.
    /// </summary>
    private readonly GenerativeFillSchemaProvider _schemaProvider = new();


    public async Task<T> FillMissingProperties<T>(string modelId, string modelClassId, T inputElement) where T : ObjectWithId, new()
    {
        var inputElements = new List<T> { inputElement };
        var response = await FillMissingProperties(modelId, modelClassId, inputElements);
        return response.Single();
    }

    public async Task<List<T>> FillMissingProperties<T>(string modelId, string modelClassId, IEnumerable<T> inputItems) where T : ObjectWithId, new()
    {
        var inputObjects = inputItems.ToList();

        // assign consecutive IDs to input elements
        for (var i = 0; i < inputObjects.Count; i++)
            inputObjects[i].Id = i + 1; // start from 1, just in case AI is trained to treat "0" differently

        // build prompt
        var inputSerialized = SerializeInput(inputObjects);
        var prompt = $"Input contains array of items to process (in the `Items` property):\n" +
                     $"\n" +
                     $"```json\n" +
                     $"{inputSerialized}\n" +
                     $"```\n" +
                     $"\n" +
                     $"The output should be in JSON and contain array of output items (with one output item for each input item, linked by Id)."
                     //+ GenerateOutputExamplePromptPart(inputObjects)
                     ;
        var schema = _schemaProvider.GenerateJsonSchemaForArrayOfItems<T>();

        var response = await generativeAiClient.GetAnswerToPrompt(modelId, modelClassId, SystemChatMessage, prompt, GenerativeAiClientResponseMode.StructuredOutput, schema);
        var resultItems = DeserializeResponse<T>(response, inputObjects.Count);

        // for each output element, rewrite values of properties without the `Fill` attribute from input elements. Match items by Id.
        RewriteInputPropertiesIntoOutput(inputObjects, resultItems);

        // return output
        return resultItems;
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
        deserializationOptions.Converters.Add(new JsonStringEnumConverter());
        var resultObject = JsonSerializer.Deserialize<ArrayOfItemsWithIds<T>>(response, deserializationOptions);
        var resultItems = resultObject.Items;

        if (resultItems.Count != numInputElements)
        {
            throw new InvalidOperationException("Number of items in response doesn't match number of items in input.");
        }

        return resultItems;
    }

    // seems not needed with Structured Outputs, but I'll test a bit more before removing this code completely
    private static string GenerateOutputExamplePromptPart<T>(List<T> inputObjects) where T : ObjectWithId, new()
    {
        // serialize output example
        var outputExample = new List<T> { inputObjects[0] };
        var outputSerializationOptions = new JsonSerializerOptions();
        outputSerializationOptions.Converters.Add(new JsonStringEnumConverter());
        outputSerializationOptions.Converters.Add(new GenerativeFillSerializationConverter<T>(SerializationSetting.IdAndOutputsPlaceholders));
        var outputExampleAsObject = new ArrayOfItemsWithIds<T>(outputExample);
        var outputFormatExample = JsonSerializer.Serialize(outputExampleAsObject, outputSerializationOptions);

        var outputExampleString = $" Example of output format with one item:" +
                                  $"\n" +
                                  $"```json\n" +
                                  $"{outputFormatExample}\n" +
                                  $"```";
        return outputExampleString;
    }

    private static void RewriteInputPropertiesIntoOutput<T>(List<T> inputElements, List<T> outputElements) where T : ObjectWithId
    {
        var properties = typeof(T).GetProperties();

        foreach (var outputElement in outputElements)
        {
            // match input element by id
            var inputElement = inputElements.Single(x => x.Id == outputElement.Id);

            foreach (var outputProperty in properties)
            {
                // leave alone properties that don't have Fill attribute; they were filled by AI model already
                var filledByAi = outputProperty.GetCustomAttribute<FillWithAIAttribute>() != null;
                if (filledByAi)
                    continue;

                // skip the Id property
                if (outputProperty.Name == "Id")
                    continue;

                // otherwise, rewrite property value from input to output
                var propertyValueInInput = outputProperty.GetValue(inputElement);
                outputProperty.SetValue(outputElement, propertyValueInInput);
            }
        }
    }
}
