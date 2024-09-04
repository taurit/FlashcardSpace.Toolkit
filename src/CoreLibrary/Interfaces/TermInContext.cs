namespace CoreLibrary.Interfaces;

/// <summary>
/// An original word to be taught on a flashcard (all in the source language).
/// </summary>
public record TermInContext(
    // e.g. "running"
    string TermOriginal,

    // e.g. "to run"
    string TermBaseForm,

    // e.g. "I like to run."
    string Sentence,

    // e.g. "verb"
    PartOfSpeech PartOfSpeech
);
