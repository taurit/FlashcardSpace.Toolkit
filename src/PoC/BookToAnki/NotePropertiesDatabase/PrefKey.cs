namespace BookToAnki.NotePropertiesDatabase;

public record PrefKey
{
    public PrefKey(string Word, string SentenceExample)
    {
        this.Word = Word.ToLowerInvariant();
        this.SentenceExample = SentenceExample;
    }

    public string Word { get; init; }
    public string SentenceExample { get; init; }
}
