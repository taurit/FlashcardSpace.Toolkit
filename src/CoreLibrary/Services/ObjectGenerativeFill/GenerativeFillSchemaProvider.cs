using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;

namespace CoreLibrary.Services.ObjectGenerativeFill;

public class GenerativeFillSchemaProvider
{
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
        generator.GenerationProviders.Add(new IncludeRulesInSchemaDescription());

        var typeOfArrayOfItems = typeof(ArrayOfItemsWithIds<TSingleItem>);

        JSchema schema = generator.Generate(typeOfArrayOfItems);
        schema.AllowAdditionalProperties = false; // required by OpenAI

        return schema.ToString();
    }
}
