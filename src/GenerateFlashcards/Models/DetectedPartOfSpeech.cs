using CoreLibrary;

namespace GenerateFlashcards.Models;

/// <summary>
/// Warning! gpt-4o-mini with Structured Outputs (!) still can return enum value outside this range. Observed by me and others.
/// Might be resolved in future models.
/// </summary>
internal enum DetectedPartOfSpeech
{
    // Easy to teach
    Noun,
    Verb,
    Adjective,

    // Weird ones
    Adverb,
    Pronoun,
    Preposition,
    Conjunction,
    Interjection,
    Article,

    WordNotRecognized,
    Other
}

internal static class EnumExtensionMethods
{
    public static PartOfSpeech ToCorePartOfSpeech(this DetectedPartOfSpeech partOfSpeech)
    {
        return partOfSpeech switch
        {
            DetectedPartOfSpeech.Noun => PartOfSpeech.Noun,
            DetectedPartOfSpeech.Verb => PartOfSpeech.Verb,
            DetectedPartOfSpeech.Adjective => PartOfSpeech.Adjective,
            DetectedPartOfSpeech.Other => PartOfSpeech.Other,
            _ => throw new ArgumentOutOfRangeException(nameof(partOfSpeech), partOfSpeech, null)
        };
    }
}
