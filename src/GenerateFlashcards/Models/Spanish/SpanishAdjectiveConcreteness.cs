using CoreLibrary.Services.ObjectGenerativeFill;

namespace GenerateFlashcards.Models.Spanish;

/// <summary>
/// Adjectives are words that modify nouns.
/// The concrete ones are easy to learn and have a clear meaning. Examples:
/// - rojo (red)
/// - grande (big)
/// - bonito (pretty)
///
/// But some are a bit unusual and don't fit well into the flashcard mode of learning. Examples:
/// - este (this)
/// - aquel (that)
/// - cada (each)
///
/// This filter is to separate the easy-to-learn adjectives from the rest.
/// </summary>
internal class SpanishAdjectiveConcreteness : ObjectWithId
{
    public string Adjective { get; init; }

    [FillWithAI]
    [FillWithAIRule("True if the adjective describes a property of an object, which is either easy to visualize, or imagine (e.g. relating to senses, emotions).")]
    [FillWithAIRule("False for abstract, demonstrative, or less concrete adjectives (e.g., este, aquel, cada).")]
    public bool IsConcrete { get; init; }

    [FillWithAI]
    [FillWithAIRule("A brief explanation of why the adjective is classified as concrete or non-concrete.")]
    public string Explanation { get; init; }
}
