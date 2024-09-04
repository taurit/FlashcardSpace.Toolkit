using System.Text.RegularExpressions;

namespace CoreLibrary.Services;

public class NormalFormProvider
{
    private readonly Dictionary<string, string> _normalizedStringsCache = new();
    private static readonly Regex ParenthesesRegex = new(@"\([^)]*\)", RegexOptions.Compiled);
    private static readonly Regex BrRegex = new(@"<br\s*/?>.*", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex HtmlTagsRegex = new("<.*?>", RegexOptions.Compiled);
    private static readonly Regex NbspRegex = new("&nbsp;", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public NormalFormProvider()
    {
        _normalizedStringsCache.EnsureCapacity(60000);
    }

    public string GetNormalizedFormOfLearnedTermWithCache(string input)
    {
        if (_normalizedStringsCache.TryGetValue(input, out var trimmed))
        {
            return trimmed;
        }

        trimmed = GetNormalizedFormOfLearnedTerm(input);
        _normalizedStringsCache[input] = trimmed;
        return trimmed;
    }

    /// <summary>
    /// Heuristically attempts to convert a flashcard content to a normalized form that can be used to:
    /// 1) look up the word in the frequency dataset
    /// 2) compare two flashcards for duplicate detection purposes
    /// 
    /// Examples:
    /// - "el teléfono" -> "teléfono"
    /// - "por un lado..." -> "por un lado"
    /// - "¡Hola!" -> "hola"
    /// - "¿Cómo?" -> "cómo"
    /// </summary>
    private static string GetNormalizedFormOfLearnedTerm(string input)
    {
        var sanitized = input;

        // replace &nbsp; with a space
        sanitized = NbspRegex.Replace(sanitized, " ");

        // remove everything after `<br />`, `<br>`, `<br/>`, `<br    />` if it's found
        sanitized = BrRegex.Replace(sanitized, "");

        // remove all HTML tags
        sanitized = HtmlTagsRegex.Replace(sanitized, string.Empty);

        // remove everything in parentheses
        sanitized = ParenthesesRegex.Replace(sanitized, "");

        // in case of multiple terms separated by a coma (like `depozyt, kaucja`), only keep the first one (here: `depozyt`)
        var indexOfComa = sanitized.IndexOf(',', StringComparison.Ordinal);
        if (indexOfComa != -1)
        {
            sanitized = sanitized.Substring(0, indexOfComa);
        }

        // lowercase
        sanitized = sanitized.ToLowerInvariant();

        // remove preceding "el", "la", "los", "las", "un", "una", "unos", "unas"
        var wordsToRemove = new[] { "el", "la", "los", "las", "un", "una", "unos", "unas", "1)", "2)", "3)", "4)" };
        foreach (var wordToRemove in wordsToRemove)
        {
            if (sanitized.StartsWith(wordToRemove + " "))
            {
                sanitized = sanitized.Substring(wordToRemove.Length + 1);
                break;
            }
        }

        // remove punctuation
        sanitized = new string(sanitized.Where(c => !char.IsPunctuation(c)).ToArray());

        // trim what's left
        var trimmed = sanitized.Trim();
        return trimmed;
    }
}
