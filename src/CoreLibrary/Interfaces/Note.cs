namespace CoreLibrary.Interfaces;

/// <summary>
/// A mutable draft of a flashcard note (in Anki's terms, i.e. it is a bag of properties
/// for a single flashcard that can be displayed in a template)
/// </summary>
public record Note(
    // e.g. "running"
    string TermOriginal,

    // e.g. "to run"
    string TermBaseForm,

    // e.g. "I like to run."
    string Sentence,

    // e.g. "verb"
    PartOfSpeech PartOfSpeech

// todo add properties for translations, image reference, audio references, etc.
)
{

    public Note(TermInContext termInContext) :
        this(termInContext.TermOriginal,
            termInContext.TermBaseForm,
            termInContext.Sentence,
            termInContext.PartOfSpeech)
    {

    }

}
