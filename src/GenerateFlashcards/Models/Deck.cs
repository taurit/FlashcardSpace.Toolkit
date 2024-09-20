using System.Text.Json.Serialization;

namespace GenerateFlashcards.Models;

internal class Deck(string deckName, List<FlashcardNote> flashcards)
{
    [JsonPropertyName("deckName")]
    public string DeckName { get; set; } = deckName;

    [JsonPropertyName("flashcards")]
    public List<FlashcardNote> Flashcards { get; set; } = flashcards;
}
