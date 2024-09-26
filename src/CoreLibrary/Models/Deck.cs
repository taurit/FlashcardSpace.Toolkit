using CoreLibrary.Interfaces;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CoreLibrary.Models;

public class Deck(string deckName, List<FlashcardNote> flashcards, string mediaFilesPrefix,
    SupportedLanguage sourceLanguage, SupportedLanguage targetLanguage)
{
    [JsonPropertyName("deckName")]
    public string DeckName { get; set; } = deckName;

    /// <summary>
    /// A short, unique prefix for file names of media files (like images and audio recordings) associated with this deck.
    /// </summary>
    [JsonPropertyName("mediaFilesPrefix")]
    public string MediaFilesPrefix { get; set; } = mediaFilesPrefix;

    [JsonPropertyName("flashcards")]
    public List<FlashcardNote> Flashcards { get; set; } = flashcards;

    [JsonPropertyName("sourceLanguage")]
    public SupportedLanguage SourceLanguage { get; set; } = sourceLanguage;

    [JsonPropertyName("targetLanguage")]
    public SupportedLanguage TargetLanguage { get; set; } = targetLanguage;

    public string Serialize() => JsonSerializer.Serialize(this, DeckSerializationOptions.SerializationOptions);
    public static Deck DeserializeFromFile(string deckFileName)
    {
        var deckSerialized = File.ReadAllText(deckFileName);
        var deserialized = JsonSerializer.Deserialize<Deck>(deckSerialized, DeckSerializationOptions.SerializationOptions);
        if (deserialized == null)
            throw new JsonException($"Failed to deserialize deck");

        // In development, I changed schema a few times, so check if the one we load contains all expected fields:
        if (deserialized.DeckName is null)
            throw new JsonException($"Deck name is missing in the deck file {deckFileName}");
        if (deserialized.MediaFilesPrefix is null)
            throw new JsonException($"Media files prefix is missing in the deck file {deckFileName}");
        if (deserialized.Flashcards is null)
            throw new JsonException($"Flashcards are missing in the deck file {deckFileName}");

        return deserialized;
    }
}
