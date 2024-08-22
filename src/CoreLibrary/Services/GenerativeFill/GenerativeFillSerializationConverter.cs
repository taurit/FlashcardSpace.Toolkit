using System.Text.Json;
using System.Text.Json.Serialization;

namespace CoreLibrary.Services.GenerativeFill;

internal enum SerializationSetting
{
    IdAndInputs,
    IdAndOutputsPlaceholders,
}

internal class GenerativeFillSerializationConverter<T>(SerializationSetting serializationSetting) : JsonConverter<T>
{
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException(); // Implement if deserialization is needed.
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        var serializePropertiesWithFillAttribute = serializationSetting == SerializationSetting.IdAndOutputsPlaceholders;
        var serializePropertiesWithoutFillAttribute = serializationSetting == SerializationSetting.IdAndInputs;

        writer.WriteStartObject();
        foreach (var prop in typeof(T).GetProperties())
        {
            var propertyHasFillAttribute = prop.IsDefined(typeof(FillWithAIAttribute), false);

            if (
                (propertyHasFillAttribute && serializePropertiesWithFillAttribute) ||
                (!propertyHasFillAttribute && serializePropertiesWithoutFillAttribute) ||
                prop.Name == "Id" // Id connects inputs and outputs and is always serialized
                )
            {
                var propValue = prop.GetValue(value);
                var propName = prop.Name;
                writer.WritePropertyName(propName);
                JsonSerializer.Serialize(writer, propValue, options);
            }
        }
        writer.WriteEndObject();
    }
}
