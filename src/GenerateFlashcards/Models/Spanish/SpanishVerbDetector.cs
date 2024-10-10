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


    [FillWithAI]
    [FillWithAIRule($"A brief sentence example with the word in the exact form given in {nameof(IsolatedWord)} used as a verb.")]
    [FillWithAIRule($"If {nameof(BaseForm)} is null, this value is also null.")]
    [FillWithAIRule("Otherwise, use A1-B1 level vocabulary suitable for wide range of student.")]
    [FillWithAIRule("The sentence should describe a scene that can be easily depicted in a picture or vividly imagined.")]
    [FillWithAIRule("Avoid abstract words and statements.")]
    public override string? SentenceExample { get; init; }
}
