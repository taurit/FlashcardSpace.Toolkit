using CoreLibrary;
using CoreLibrary.Services.ObjectGenerativeFill;
using System.ComponentModel.DataAnnotations;

namespace GenerateFlashcards.Models.Spanish;
internal class SpanishTermWithPolishTranslation : ObjectWithId
{
    [Key]
    public string SpanishWord { get; init; }
    public string SpanishSentence { get; init; }

    // should help e.g. determine if we "perezoso" as adjective or noun, and translate it accordingly
    public PartOfSpeech SpanishWordPartOfSpeech { get; init; }

    [FillWithAI]
    [FillWithAIRule($"A translation of {nameof(SpanishWord)} (as used in the context of {nameof(SpanishSentence)}) to Polish")]
    public string SpanishWordEquivalentInPolish { get; init; }

    [FillWithAI]
    [FillWithAIRule($"A translation of {nameof(SpanishSentence)} to Polish")]
    public string SpanishSentenceEquivalentInPolish { get; init; }
}
