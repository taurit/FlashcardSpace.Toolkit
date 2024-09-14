using CoreLibrary.Services.ObjectGenerativeFill;
using System.Diagnostics;

namespace GenerateFlashcards.Models.Spanish;

[DebuggerDisplay("#{Id} {IsolatedWord} -> {IsAdjective}, {BaseForm}")]
internal class SpanishAdjectiveDetector : PartOfSpeechDetector
{
    [FillWithAI]
    [FillWithAIRule($"True if the {nameof(IsolatedWord)} can be used as an adjective (adjetivo) in a grammatically correct Spanish sentence.")]
    [FillWithAIRule("Don't assume typos or errors. If the word doesn't fit the rule, return false.")]
    public bool IsAdjective { get; init; }

    [FillWithAI]
    [FillWithAIRule("If the word can be used as adjective, this value contains a singular masculine form of it (e.g., for 'verdes' it's 'verde').")]
    public override string? BaseForm { get; init; }


}
