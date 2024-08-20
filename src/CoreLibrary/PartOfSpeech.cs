namespace CoreLibrary;

public enum PartOfSpeech
{
    /// <summary>
    /// Not determined yet
    /// </summary>
    Unknown,

    Noun,
    Verb,
    Adjective,

    /// <summary>
    /// Strictly speaking not a part of speech, but it's a useful category besides Nouns, Verbs, and Adjectives
    /// that is well suited for use in Flashcards. 
    /// </summary>
    Idiom,

    Other
}
