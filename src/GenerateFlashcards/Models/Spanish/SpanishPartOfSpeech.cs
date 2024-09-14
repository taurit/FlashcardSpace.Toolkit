using CoreLibrary;

namespace GenerateFlashcards.Models.Spanish;

/// <summary>
/// Warning! gpt-4o-mini with Structured Outputs (!) still can return enum value outside this range. Observed by me and others.
/// Might be resolved in future models.
/// </summary>
internal enum SpanishPartOfSpeech
{
    // Fallback because for some words like `este`, or auxiliary verbs like 'haber', the model doesn't want to use
    // any predefined value and does weird things (skips the word, or makes up a new part of speech).
    // Warning: keep it as the first value, because it's used as a fallback in deserialization of OpenAI response.
    Unknown = 0,

    // Noun
    Sustantivo,
    // Verb
    Verbo,
    // Adjective
    Adjetivo,
    // Adverb
    Adverbio,
    // Pronoun
    Pronombre,
    // Determiner (including articles)
    Determinante,
    // Conjunction
    Conjuncion,
    // Interjection
    Interjeccion,
    // Numeral
    Numeral,
    // Preposition
    Preposicion
}

internal static class SpanishPartOfSpeechExtensionMethods
{
    public static PartOfSpeech ToCorePartOfSpeech(this SpanishPartOfSpeech partOfSpeech)
    {
        return partOfSpeech switch
        {
            SpanishPartOfSpeech.Sustantivo => PartOfSpeech.Noun,
            SpanishPartOfSpeech.Verbo => PartOfSpeech.Verb,
            SpanishPartOfSpeech.Adjetivo => PartOfSpeech.Adjective,
            _ => PartOfSpeech.Other
        };
    }
}
