using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CoreLibrary.Services.ObjectGenerativeFill;

/// <summary>
/// Filters out input properties that are not expected in the output (and should not be present in the output schema)
/// </summary>
/// <typeparam name="TSingleItem">
/// This is to only filter out properties for the type of the item in the array, not for the array itself or deeper types.
/// </typeparam>
public class FilterOutInputProperties<TSingleItem> : DefaultContractResolver
{
    protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
    {
        var allProperties = base.CreateProperties(type, memberSerialization);

        if (type == typeof(TSingleItem))
        {
            var propertiesToIncludeInSchema = allProperties
                .Where(p => p.AttributeProvider.GetAttributes(typeof(FillWithAIAttribute), true).Any() || p.PropertyName == nameof(ObjectWithId.Id))
                .ToList();
            return propertiesToIncludeInSchema;
        }

        return allProperties;
    }
}
