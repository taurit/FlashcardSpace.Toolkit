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
    public static Deck DeserializeFromFile(string deckFileName)
    {
        var deckSerialized = File.ReadAllText(deckFileName);
        var deserialized = JsonSerializer.Deserialize<Deck>(deckSerialized, DeckSerializationOptions.SerializationOptions);
        if (deserialized == null)
            throw new JsonException($"Failed to deserialize deck");

        return deserialized;
    }
}
