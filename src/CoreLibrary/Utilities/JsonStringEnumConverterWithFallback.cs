using System.Text.Json;
using System.Text.Json.Serialization;

namespace CoreLibrary.Utilities;

/// <summary>
/// Source: https://github.com/dotnet/runtime/issues/57031#issuecomment-2196647098
/// </summary>
internal class JsonStringEnumConverterWithFallback : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert) => typeToConvert.IsEnum;

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var closedType = typeof(ErrorHandlingEnumConverter<>).MakeGenericType(typeToConvert);
        return (JsonConverter?)Activator.CreateInstance(closedType);
    }

    private class ErrorHandlingEnumConverter<T> : JsonConverter<T> where T : struct
    {
        public override bool CanConvert(Type typeToConvert)
            => typeToConvert.IsEnum;

        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String)
            {
                return default;
            }

            var enumString = reader.GetString();
            return Enum.TryParse<T>(enumString, ignoreCase: true, out var result)
                ? result
                : default;
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
            => throw new NotImplementedException();
    }
}
