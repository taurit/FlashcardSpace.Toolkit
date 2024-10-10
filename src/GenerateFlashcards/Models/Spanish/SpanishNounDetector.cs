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

    [FillWithAI]
    [FillWithAIRule($"A brief sentence example with the word in the exact form given in {nameof(IsolatedWord)} used as a noun.")]
    [FillWithAIRule($"If {nameof(BaseForm)} is null, this value is also null.")]
    [FillWithAIRule("Otherwise, use A1-B1 level vocabulary suitable for wide range of student.")]
    [FillWithAIRule("The sentence should describe a scene that can be easily depicted in a picture or vividly imagined.")]
    [FillWithAIRule("Avoid abstract words and statements.")]
    public override string? SentenceExample { get; init; }
}
