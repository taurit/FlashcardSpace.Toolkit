using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace CoreLibrary.Models;

public static class DeckSerializationOptions
{
    public static readonly JsonSerializerOptions SerializationOptions = new JsonSerializerOptions
    {
        // For readability
        WriteIndented = true,

        // Don't save null values
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,

        // Add JsonStringEnumConverter to serialize enums as strings
        Converters = { new JsonStringEnumConverter(allowIntegerValues: false) },

        // Required to generate schema
        TypeInfoResolver = new DefaultJsonTypeInfoResolver() // enums dont work


    };

}
