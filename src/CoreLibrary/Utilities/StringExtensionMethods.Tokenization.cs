namespace CoreLibrary.Utilities;

public static class StringExtensionMethodsTokenization
{
    /// <summary>
    /// While tokenizing book into sentences, we don't want to lose ellipsis ("...") as it adds meaning to a sentence.
    /// But it's easier to treat it as a punctuation mark if it is represented as a single UTF character, and not several.
    /// </summary>
    public static string ReplaceEllipsisWithSingleCharacter(this string bookContent)
    {
        return bookContent
                .Replace("...", "…")
                .Replace("..", ".")
                .Replace("….", "…")
                .Replace(" …", "…")
            ;

    }
}
