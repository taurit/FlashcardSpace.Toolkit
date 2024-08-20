namespace CoreLibrary;

/// <summary>
/// A mutable object representing a flashcards Note (as in Anki Note) and metadata 
/// </summary>
public record Note
{
    public string Word { get; set; }
    public List<string> SentencesWhereWordIsFound { get; set; }
}
