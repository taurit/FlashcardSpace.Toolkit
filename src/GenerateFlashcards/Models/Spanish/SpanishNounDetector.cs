using CoreLibrary.Services.ObjectGenerativeFill;
using System.Diagnostics;

namespace GenerateFlashcards.Models.Spanish;

[DebuggerDisplay("#{Id} {IsolatedWord} -> {IsNoun}, {BaseForm}")]
internal class SpanishNounDetector : PartOfSpeechDetector
{
    [FillWithAI]
    [FillWithAIRule($"True if the {nameof(IsolatedWord)} can be used as a noun (sustantivo) in a grammatically correct Spanish sentence.")]
    [FillWithAIRule("Don't assume typos or errors. If the word doesn't fit the rule, return false.")]
    public bool IsNoun { get; init; }

    [FillWithAI]
    [FillWithAIRule("If the word can be used as noun, this value contains a singular masculine form of it, always with a definite article (e.g., for 'gatos' it's 'el gato').")]
    public override string? BaseForm { get; init; }
}
