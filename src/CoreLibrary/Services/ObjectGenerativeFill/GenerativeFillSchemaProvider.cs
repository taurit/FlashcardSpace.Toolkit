using CoreLibrary.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;

namespace CoreLibrary.Services.ObjectGenerativeFill;

public class GenerativeFillSchemaProvider
{
    private readonly string _generativeFillCacheFolder;

    public GenerativeFillSchemaProvider(string generativeFillCacheFolder)
    {
        _generativeFillCacheFolder = generativeFillCacheFolder;

        // Ensure the folder exists (does nothing if it exists already
        Directory.CreateDirectory(generativeFillCacheFolder);
    }

    /// <summary>
    /// Generates expected JSON schema for a ChatGPT containing an array of items of specific type.
    /// The response is wrapped in an object with the `Items` property. Example of a response:
    /// <example>
    /// {
    ///   "Items": [
    ///     { "Id": 1, "Property1": "value1A", "Property2": "value2A" }
    ///     { "Id": 2, "Property1": "value1B", "Property2": "value2B" }
    ///   ]
    /// }
    /// </example>
    /// <typeparam name="TSingleItem">Type of item in the response</typeparam>
    /// </summary>
    public string GenerateJsonSchemaForArrayOfItems<TSingleItem>() where TSingleItem : ObjectWithId
    {
        // Todo: starting with .NET 9 it will probably be possible to replace it with the native System.Text.Json schema generator
        // https://github.com/openai/openai-dotnet/issues/160#issuecomment-2391863242
        // and remove all the hackish code for caching JSON schema to avoid hitting throttling limits of Newtonsoft library
        // Alternatively, here's one more library created later just for this purpose: https://github.com/r-Larch/OpenAi.JsonSchema

        JSchemaGenerator generator = new JSchemaGenerator();
        generator.ContractResolver = new FilterOutInputProperties<TSingleItem>();
        generator.DefaultRequired = Required.Always; // required by OpenAI
        generator.SchemaReferenceHandling = SchemaReferenceHandling.None;
        generator.SchemaIdGenerationHandling = SchemaIdGenerationHandling.TypeName;
        generator.GenerationProviders.Add(new StringEnumGenerationProvider());
        generator.GenerationProviders.Add(new IncludeRulesInSchemaDescription());

        var typeOfArrayOfItems = typeof(ArrayOfItemsWithIds<TSingleItem>);
        var typeOfSingleItem = typeof(TSingleItem);

        // have cache expire after any changes in AI prompt defined in attributes
        var typeFingerprint = ClassFingerprintProvider.GenerateTypeFingerprint(typeOfSingleItem);

        var schemaCacheFileName = $"schema-" +
                                  $"{typeOfSingleItem.Name.ToFilenameFriendlyString()}-" +
                                  $"array-" +
                                  $"f{typeFingerprint.GetHashCodeStable(4)}" +
                                  $".json";

        var schemaCacheFilePath = Path.Combine(_generativeFillCacheFolder, schemaCacheFileName);

        if (!File.Exists(schemaCacheFilePath))
        {
            JSchema schema = generator.Generate(typeOfArrayOfItems);
            schema.AllowAdditionalProperties = false; // required by OpenAI
            var schemaAsString = schema.ToString();

            // remove indentation
            var jsonObject = JsonConvert.DeserializeObject(schemaAsString);
            string nonIndentedJson = JsonConvert.SerializeObject(jsonObject, Formatting.None);

            File.WriteAllText(schemaCacheFilePath, nonIndentedJson);
        }

        var schemaString = File.ReadAllText(schemaCacheFilePath);

        return schemaString;
    }
}
