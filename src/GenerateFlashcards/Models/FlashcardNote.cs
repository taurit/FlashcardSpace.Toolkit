using CoreLibrary;
using System.Text.Json.Serialization;

namespace GenerateFlashcards.Models;

internal class FlashcardNote
{
    [JsonPropertyName("term")]
    public string Term { get; set; }

    [JsonPropertyName("termAudio")]
    public string TermAudio { get; set; }

    [JsonPropertyName("termTranslation")]
    public string TermTranslation { get; set; }

    [JsonPropertyName("termTranslationAudio")]
    public string TermTranslationAudio { get; set; }

    [JsonPropertyName("termDefinition")]
    public string TermDefinition { get; set; }

    [JsonPropertyName("context")]
    public string Context { get; set; }

    [JsonPropertyName("contextTranslation")]
    public string ContextTranslation { get; set; }

    [JsonPropertyName("type")]
    public PartOfSpeech Type { get; set; }

    [JsonPropertyName("imageCandidates")]
    public List<string> ImageCandidates { get; set; } = new List<string>();
}
