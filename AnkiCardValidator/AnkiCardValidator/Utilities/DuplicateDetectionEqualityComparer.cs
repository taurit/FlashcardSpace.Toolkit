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

        var xNormalized = ConvertToComparableForm(x);
        var yNormalized = ConvertToComparableForm(y);
        return xNormalized == yNormalized;
    }

    public int GetHashCode(string obj)
    {
        return ConvertToComparableForm(obj).GetHashCode();
    }

    private string ConvertToComparableForm(string input)
    {
        if (!_normalizedStringsCache.ContainsKey(input))
        {
            // remove everything after `<br />` if it's found
            var indexOfBr = input.IndexOf("<br />", StringComparison.OrdinalIgnoreCase);
            if (indexOfBr != -1)
            {
                input = input.Substring(0, indexOfBr);
            }

            // remove everything in parentheses
            var sanitized = Regex.Replace(input, @"\([^)]*\)", "");

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

            _normalizedStringsCache[input] = trimmed;
        }

        return _normalizedStringsCache[input];
    }
}
