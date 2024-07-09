using System.Text.RegularExpressions;

namespace AnkiCardValidator.Utilities;

public class DuplicateDetectionEqualityComparer : IEqualityComparer<string>
{
    private readonly Dictionary<string, string> _normalizedStringsCache = new();

    public bool Equals(string? x, string? y)
    {
        if (x is null || y is null)
        {
            return false;
        }

        var xNormalized = GetNormalizedFormOfLearnedTermWithCache(x);
        var yNormalized = GetNormalizedFormOfLearnedTermWithCache(y);
        return xNormalized == yNormalized;
    }

    public int GetHashCode(string obj)
    {
        return GetNormalizedFormOfLearnedTermWithCache(obj).GetHashCode();
    }

    internal string GetNormalizedFormOfLearnedTermWithCache(string input)
    {
        if (!_normalizedStringsCache.ContainsKey(input))
        {
            var trimmed = GetNormalizedFormOfLearnedTerm(input);

            _normalizedStringsCache[input] = trimmed;
        }

        return _normalizedStringsCache[input];
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

        // remove everything after `<br />` if it's found
        var indexOfBr = sanitized.IndexOf("<br />", StringComparison.OrdinalIgnoreCase);
        if (indexOfBr != -1)
        {
            sanitized = sanitized.Substring(0, indexOfBr);
        }

        // remove everything in parentheses
        sanitized = Regex.Replace(sanitized, @"\([^)]*\)", "");

        // in case of multiple terms separated by a coma (like `depozyt, kaucja`), only keep the first one (here: `depozyt`)
        var indexOfComa = sanitized.IndexOf(",", StringComparison.Ordinal);
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
            }
        }

        // remove punctuation
        sanitized = new string(sanitized.Where(c => !char.IsPunctuation(c)).ToArray());

        // trim what's left
        var trimmed = sanitized.Trim();
        return trimmed;
    }
}
