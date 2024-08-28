using CoreLibrary;

namespace GenerateFlashcards.Models;

internal enum DetectedPartOfSpeech
{
    Noun,
    Verb,
    Adjective,
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
