using System.IO;
using System.Reflection;
using System.Text.Json;

namespace AnkiCardValidator.Utilities.JsonGenerativeFill;

/// <summary>
/// Helps process arrays of items instead of single item, to save on input tokens
/// </summary>
public static class JsonGenerativeFill
{
    public static async Task<List<T>> GetAnswersToPromptsUsingChatGptApi<T>(
        string hintsOnHowToTransformSingleItem,
        List<T> inputElements,
        string systemChatMessage = "You are a helpful assistant"
        ) where T : ItemWithId
    {
        // assign consecutive IDs to input elements
        for (var i = 0; i < inputElements.Count; i++)
        {
            inputElements[i].Id = i + 1; // start from 1, just in case AI is trained to treat "0" differently
        }

        // build prompt

        // serialize input to process
        var inputSerializationOptions = new JsonSerializerOptions();
        inputSerializationOptions.Converters.Add(new GenerativeFillSerializationConverter<T>(SerializationSetting.IdAndInputs));
        // wrapping array with an object because OpenAI API doesn't like JSON arrays as root element
        var inputArrayAsObject = new ArrayOfItemsWithIds<T>(inputElements);
        var inputSerialized = JsonSerializer.Serialize(inputArrayAsObject, inputSerializationOptions);

        // serialize output example
        var outputExample = new List<T> { inputElements[0] };
        var outputSerializationOptions = new JsonSerializerOptions();
        outputSerializationOptions.Converters.Add(new GenerativeFillSerializationConverter<T>(SerializationSetting.IdAndOutputsPlaceholders));
        var outputExampleAsObject = new ArrayOfItemsWithIds<T>(outputExample);
        var outputFormatExample = JsonSerializer.Serialize(outputExampleAsObject, outputSerializationOptions);

        var prompt = $"Input contains array of items to process (in the `items` property):\n" +
                     $"\n" +
                     $"```json\n" +
                     $"{inputSerialized}\n" +
                     $"```\n" +
                     $"\n" +
                     $"The output should be in JSON and contain array of output items with following schema:\n" +
                     $"\n" +
                     $"```json\n" +
                     $"{outputFormatExample}\n" +
                     $"```\n" +
                     $"\n" +
                     $"For each input item you should generate one output item, using the `id` property as a key linking input and output.\n" +
                     $"Your job is to replace the null values with content. Hints on how to best fill the missing data:\n" +
                     $"{hintsOnHowToTransformSingleItem}";


        // execute query
        var responseFileName = await ChatGptHelper.GetAnswerToPromptUsingChatGptApi(systemChatMessage, prompt, 1, true);
        var response = await File.ReadAllTextAsync(responseFileName);

        // match items in response array with items in input array
        // deserialize response
        var resultObject = JsonSerializer.Deserialize<ArrayOfItemsWithIds<T>>(response);
        var resultItems = resultObject.Items;

        if (resultItems.Count != inputElements.Count)
        {
            throw new InvalidOperationException("Number of items in response doesn't match number of items in input.");
        }

        // for each output element, rewrite values of properties without the `Fill` attribute from input elements. Match items by Id.
        RewriteInputPropertiesIntoOutput(inputElements, resultItems);

        // return output
        return resultItems;
    }

    private static void RewriteInputPropertiesIntoOutput<T>(List<T> inputElements, List<T> outputElements) where T : ItemWithId
    {
        var properties = typeof(T).GetProperties();

        foreach (var outputElement in outputElements)
        {
            // match input element by id
            var inputElement = inputElements.Single(x => x.Id == outputElement.Id);

            foreach (var outputProperty in properties)
            {
                // leave alone properties that don't have Fill attribute; they were filled by AI model already
                var filledByAi = outputProperty.GetCustomAttribute<FillAttribute>() != null;
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
