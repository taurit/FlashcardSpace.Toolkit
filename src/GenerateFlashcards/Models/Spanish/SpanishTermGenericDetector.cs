using CoreLibrary.Services.ObjectGenerativeFill;

namespace GenerateFlashcards.Models.Spanish;

/// <summary>
/// Experimental model to transform very loose hints about word I want to learn into data for flashcards.
/// Unlike other models, here I assume that:
/// - input is a hint that might be in Polish, Spanish or mixed (no guarantees)
/// - input might be a single word, a phrase, an idiom, a phrase with some *specific word* highlighted to focus on it
/// - input might contain typos that need to be corrected
/// </summary>
internal class SpanishTermGenericDetector : ObjectWithId
{
    public string TermToCreateFlashcardFor { get; init; }

    [FillWithAI]
    [FillWithAIRule($"While {nameof(TermToCreateFlashcardFor)} contains a hint on what I want to learn, this field contains a proposed front side of a flashcard.")]
    [FillWithAIRule("This value should be a word or a short phrase in Spanish.")]
    [FillWithAIRule($"Please note that {nameof(TermToCreateFlashcardFor)} might contain content in Spanish, Polish, or mix languages.")]
    [FillWithAIRule($"{nameof(TermToCreateFlashcardFor)} might contain typos, so this field should be corrected if needed.")]
    [FillWithAIRule($"If {nameof(TermToCreateFlashcardFor)} contains a fragment that is marked out using star characters (*bold text*), focus on teaching the highlighted content and not other words.")]
    [FillWithAIRule($"If a single word is taught, precede it with a definite article (el or la). Standardize to singular form (rather than plural) if such form exists.")]
    public string FlashcardQuestionInSpanish { get; init; }

    [FillWithAI]
    [FillWithAIRule($"A part of the back side of the flashcard, and a translation of {nameof(FlashcardQuestionInSpanish)} in the context of the example sentence.")]
    [FillWithAIRule("This value should be in Polish")]
    public string FlashcardAnswerInPolish { get; init; }

    [FillWithAI]
    [FillWithAIRule($"A part of the back side of the flashcard, and a translation of {nameof(FlashcardQuestionInSpanish)} in the context of the example sentence.")]
    [FillWithAIRule("This value should be completely in Polish")]
    public string FlashcardAnswerInEnglish { get; init; }

    [FillWithAI]
    [FillWithAIRule($"An example of a sentence using the term taught in {nameof(FlashcardQuestionInSpanish)}.")]
    [FillWithAIRule("This value should be completely in Spanish.")]
    [FillWithAIRule($"If {nameof(TermToCreateFlashcardFor)} contains a sentence already, use it, but first check for errors and refine if needed.")]
    public string FlashcardExampleSentenceInSpanish { get; init; }

    [FillWithAI]
    [FillWithAIRule($"If the term taught in {nameof(TermToCreateFlashcardFor)} is unusual for Spanish (Castellano), or rarely used in Europe, briefly explain why.")]
    [FillWithAIRule("If there is a word with the same meaning used more frequently, mention it.")]
    [FillWithAIRule("If the word has multiple frequently used meanings, mention it.")]
    public string? Remarks { get; init; }

    [FillWithAI]
    [FillWithAIRule($"Brief explanation of why other fields were generated the way they were.")]
    public string? Explanation { get; init; }

}
