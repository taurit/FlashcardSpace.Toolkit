using CoreLibrary.Services.GenerativeAiClients;
using System.Reflection;
using System.Text.Json;

namespace CoreLibrary.Services.GenerativeFill;

/// <summary>
/// Helps process arrays of items instead of single item, to save on input tokens
/// </summary>
public class GenerativeFill(IGenerativeAiClient generativeAiClient)
{
    const string SystemChatMessage = "You are a helpful assistant";

    public async Task<T> FillMissingProperties<T>(string modelId, string modelClassId, T inputElement) where T : ObjectWithId
    {
        var inputElements = new List<T> { inputElement };
        var response = await FillMissingProperties(modelId, modelClassId, inputElements);
        return response.Single();
    }

    public async Task<List<T>> FillMissingProperties<T>(string modelId, string modelClassId, IEnumerable<T> inputItems) where T : ObjectWithId
    {
        var inputObjects = inputItems.ToList();

        // assign consecutive IDs to input elements
        for (var i = 0; i < inputObjects.Count; i++)
        {
            inputObjects[i].Id = i + 1; // start from 1, just in case AI is trained to treat "0" differently
        }

        // build prompt

        // serialize input to process
        var inputSerializationOptions = new JsonSerializerOptions();
        inputSerializationOptions.Converters.Add(new GenerativeFillSerializationConverter<T>(SerializationSetting.IdAndInputs));
        // wrapping array with an object because OpenAI API doesn't like JSON arrays as root element
        var inputArrayAsObject = new ArrayOfItemsWithIds<T>(inputObjects);
        var inputSerialized = JsonSerializer.Serialize(inputArrayAsObject, inputSerializationOptions);

        // serialize output example
        var outputExample = new List<T> { inputObjects[0] };
        var outputSerializationOptions = new JsonSerializerOptions();
        outputSerializationOptions.Converters.Add(new GenerativeFillSerializationConverter<T>(SerializationSetting.IdAndOutputsPlaceholders));
        var outputExampleAsObject = new ArrayOfItemsWithIds<T>(outputExample);
        var outputFormatExample = JsonSerializer.Serialize(outputExampleAsObject, outputSerializationOptions);

        var hints = GenerateHintsPart(typeof(T));

        var prompt = $"Input contains array of items to process (in the `Items` property):\n" +
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
                     $"Your job is to replace the null values with content.\n" +
                     $"\n" +
                     $"{hints}";


        // execute query
        var response = await generativeAiClient.GetAnswerToPrompt(modelId, modelClassId, SystemChatMessage, prompt, true);

        // match items in response array with items in input array
        // deserialize response
        var resultObject = JsonSerializer.Deserialize<ArrayOfItemsWithIds<T>>(response);
        var resultItems = resultObject.Items;

        if (resultItems.Count != inputObjects.Count)
        {
            throw new InvalidOperationException("Number of items in response doesn't match number of items in input.");
        }

        // for each output element, rewrite values of properties without the `Fill` attribute from input elements. Match items by Id.
        RewriteInputPropertiesIntoOutput(inputObjects, resultItems);

        // return output
        return resultItems;
    }

    private static string GenerateHintsPart(Type type)
    {
        // scan all properties of the type given as argument and collect all instances of [FillWithAIRuleAttribute] in a list
        var properties = type.GetProperties();
        var rulesSpecifiedInAttributes = new List<string>();

        foreach (var property in properties)
        {
            var rulesForProperty = property.GetCustomAttributes<FillWithAIRuleAttribute>();
            foreach (var rule in rulesForProperty)
            {
                var ruleForPrompt = $"For `{property.Name}` property: {rule.RuleText}";
                rulesSpecifiedInAttributes.Add(ruleForPrompt);
            }

        }

        string rulesString = "Try deduce the meaning of properties to fill without any hints.";

        if (rulesSpecifiedInAttributes.Any())
        {
            var rulesPrecededByBulletPoint = rulesSpecifiedInAttributes.Select(rule => $"- {rule}");
            var rulesAsSingleString = String.Join("\n", rulesPrecededByBulletPoint);
            rulesString = $"Use the following rules when filling values of properties:\n" +
                          $"{rulesAsSingleString}";
        }
        return rulesString;

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
