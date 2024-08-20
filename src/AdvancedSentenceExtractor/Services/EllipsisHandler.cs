namespace AdvancedSentenceExtractor.Services;
public static class EllipsisHandler
{
    /// <summary>
    /// We don't want to lose ellipsis ("...") while tokenizing book into sentences, as it adds meaning to a sentence.
    /// But it's easier to treat it as a punctuation mark if it is represented as a single UTF character.
    /// </summary>
    public static string ReplaceEllipsisWithSingleCharacter(string bookContent)
    {
        return bookContent
            .Replace("...", "…")
            .Replace("..", ".")
            .Replace("….", "…")
            .Replace(" …", "…")
            ;

    }

}
