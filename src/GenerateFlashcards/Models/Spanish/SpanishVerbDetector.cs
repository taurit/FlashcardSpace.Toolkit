using CoreLibrary.Services.ObjectGenerativeFill;
using System.Diagnostics;

namespace GenerateFlashcards.Models.Spanish;

[DebuggerDisplay("#{Id} {IsolatedWord} -> {IsVerb}, {BaseForm}")]
internal class SpanishVerbDetector : PartOfSpeechDetector
{
    [FillWithAI]
    [FillWithAIRule($"True if the {nameof(IsolatedWord)} can be used as a verb (verbo) in a grammatically correct Spanish sentence.")]
    [FillWithAIRule("Don't assume typos or errors. If the word doesn't fit the rule, return false.")]
    public bool IsVerb { get; init; }

    [FillWithAI]
    [FillWithAIRule($"If {nameof(IsVerb)} is true, contains the infinitive form (e.g., for 'comiendo' it's 'comer'). Otherwise, it's null.")]
    public override string? BaseForm { get; init; }

}
