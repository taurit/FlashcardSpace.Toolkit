using System.Text.Json;
using System.Text.Json.Serialization;

namespace CoreLibrary.Models;

internal static class DeckSerializationOptions
{
    internal static readonly JsonSerializerOptions SerializationOptions = new JsonSerializerOptions
    {
        // For readability
        WriteIndented = true,

        // Don't save null values
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,

        // Add JsonStringEnumConverter to serialize enums as strings
        Converters = { new JsonStringEnumConverter() }
    };

}
