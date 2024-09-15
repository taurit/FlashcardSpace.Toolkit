using CoreLibrary.Services.ObjectGenerativeFill;
using System.ComponentModel.DataAnnotations;

namespace GenerateFlashcards.Models.Spanish;
internal class SpanishTermWithEnglishTranslation : ObjectWithId
{
    [Key]
    public string SpanishWord { get; init; }
    public string SpanishSentence { get; init; }

    [FillWithAI]
    [FillWithAIRule($"A translation of {nameof(SpanishWord)} (as used in the context of {nameof(SpanishSentence)}) to English (US)")]
    public string SpanishWordEquivalentInEnglish { get; init; }

    [FillWithAI]
    [FillWithAIRule($"A translation of {nameof(SpanishSentence)} to English (US)")]
    public string SpanishSentenceEquivalentInEnglish { get; init; }
}
