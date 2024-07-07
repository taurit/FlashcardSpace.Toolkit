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
            // remove punctuation
            var sanitized = new string(input.Where(c => !char.IsPunctuation(c)).ToArray());

            // remove leading/trailing whitespaces
            sanitized = sanitized.Trim();

            // lowercase
            var lowercase = sanitized.ToLowerInvariant();

            // remove preceding articles
            var wordsToRemove = new[] { "el", "la", "los", "las", "un", "una", "unos", "unas" };
            foreach (var wordToRemove in wordsToRemove)
            {
                if (lowercase.StartsWith(wordToRemove + " "))
                {
                    lowercase = lowercase.Substring(wordToRemove.Length + 1);
                }
            }

            // trim what's left
            var trimmed = lowercase.Trim();
            _normalizedStringsCache[input] = trimmed;
        }

        return _normalizedStringsCache[input];
    }
}
