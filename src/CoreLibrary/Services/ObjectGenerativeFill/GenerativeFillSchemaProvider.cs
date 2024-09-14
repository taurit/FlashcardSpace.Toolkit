using CoreLibrary.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;
using System.Reflection;
using System.Text;

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
        var typeFingerprint = GenerateTypeFingerprint(typeOfSingleItem);

        var schemaCacheFileName = $"schema-" +
                                  $"arr-" +
                                  $"{typeFingerprint.GetHashCodeStable(4)}" +
                                  $"-" +
                                  $"{typeOfSingleItem.FullName!.GetHashCodeStable(4)}" +
                                  $"-" +
                                  $"{typeOfSingleItem.Name.GetFilenameFriendlyString()}" +
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

    /// <summary>
    /// This method generates unique string fingerprint for the type of single item.
    /// The fingerprint should change whenever:
    /// - any of the property names changes
    /// - properties order change
    /// - presence of [FillWithAI] attribute on any property changed
    /// - presence of [FillWithAIRule] attribute on any property changed
    /// - the content of [FillWithAIRule] attribute changed
    /// </summary>
    /// <param name="typeOfSingleItem">A type to generate the fingerprint for</param>
    /// <returns>An arbitrary string that wil remain the same if type didn't change, but differs if it changed.</returns>
    private string GenerateTypeFingerprint(Type typeOfSingleItem)
    {
        var properties = typeOfSingleItem.GetProperties();
        var fingerprint = new StringBuilder();

        foreach (var property in properties)
        {
            fingerprint.AppendLine($"Property: {property.Name}");
            fingerprint.AppendLine($"Has FillWithAIAttribute: {property.GetCustomAttribute<FillWithAIAttribute>() != null}");

            var ruleAttributes = property.GetCustomAttributes<FillWithAIRuleAttribute>();
            foreach (var ruleAttribute in ruleAttributes)
                fingerprint.AppendLine($"FillWithAIRuleAttribute text: {ruleAttribute.RuleText}");

            fingerprint.AppendLine($"");
        }

        var generateTypeFingerprint = fingerprint.ToString();
        return generateTypeFingerprint;
    }
}
