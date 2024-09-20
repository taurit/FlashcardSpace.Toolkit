using System.Text.Json;
using System.Text.Json.Serialization;

namespace CoreLibrary.Models;

public class Deck(string deckName, List<FlashcardNote> flashcards)
{
    [JsonPropertyName("deckName")]
    public string DeckName { get; set; } = deckName;

    [JsonPropertyName("flashcards")]
    public List<FlashcardNote> Flashcards { get; set; } = flashcards;

    public string Serialize() => JsonSerializer.Serialize(this, DeckSerializationOptions.SerializationOptions);
    public static Deck Deserialize(string deckSerialized)
    {
        var deserialized = JsonSerializer.Deserialize<Deck>(deckSerialized, DeckSerializationOptions.SerializationOptions);
        if (deserialized == null)
            throw new JsonException($"Failed to deserialize deck");

        return deserialized;
    }
}

internal static class DeckSerializationOptions
{
    internal static readonly JsonSerializerOptions SerializationOptions = new JsonSerializerOptions
    {
        // For readability
        WriteIndented = true,

        // Add JsonStringEnumConverter to serialize enums as strings
        Converters = { new JsonStringEnumConverter() }
    };

}
