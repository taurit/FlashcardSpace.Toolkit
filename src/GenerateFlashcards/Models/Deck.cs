using System.Text.Json.Serialization;

namespace GenerateFlashcards.Models;

internal class Deck
{
    // todo: TermInContext is an early stage model. To be replaced with later-stage model when I develop it
    [JsonPropertyName("flashcards")]
    public List<FlashcardNote> Flashcards { get; set; }
}
