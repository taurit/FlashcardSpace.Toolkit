using CoreLibrary;

namespace GenerateFlashcards.Models.Spanish;

internal enum EnglishPartOfSpeech
{
    // -- Easy to teach -- 

    Noun,
    Verb,
    Adjective,

    // -- The rest -- 
    Other
}

internal static class EnglishPartOfSpeechExtensionMethods
{
    public static PartOfSpeech ToCorePartOfSpeech(this EnglishPartOfSpeech partOfSpeech)
    {
        return partOfSpeech switch
        {
            EnglishPartOfSpeech.Noun => PartOfSpeech.Noun,
            EnglishPartOfSpeech.Verb => PartOfSpeech.Verb,
            EnglishPartOfSpeech.Adjective => PartOfSpeech.Adjective,
            _ => PartOfSpeech.Other
        };
    }
}
