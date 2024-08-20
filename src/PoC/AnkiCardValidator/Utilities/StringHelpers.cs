using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace AnkiCardValidator.Utilities;
public static partial class StringHelpers
{
    // https://en.wikipedia.org/wiki/Most_common_words_in_Spanish + own experience of words common in language learning like nuestro
    private static readonly string[] CommonSpanishWords = ["de", "la", "se", "los", "las", "con", "el", "en", "le", "que", "y", "del", "un", "por", "con", "una", "su", "para", "es", "al", "lo", "como", "pero", "mi", "si", "me", "fue", "era", "han", "hay", "yo", "nuestro", "vuestro", "vosotros", "nosotros", "ellos", "ella", "te"];
    private static readonly string[] CommonPolishWords = ["nie", "to", "się", "na", "co", "jest", "do", "tak", "jak", "mnie", "za", "ja", "ci", "tu", "go", "tym", "ty", "czy", "tylko", "po", "jestem", "ma", "w"];

    public static bool IsStringLikelyInSpanishLanguage(String query)
    {
        // "Óó" is shared by Polish and Spanish, so let's try heuristics
        // "kroplówka" still was Spanish... I'll change to be more conservative and treat "ó" as non-decisive letter
        var containsPolishCharacters = "ąćęłńśźżĄĆĘŁŃŚŻŹ".Any(query.Contains);

        // this and further are performance optimization to not run costly functions unless necessary - this was a bottleneck
        if (containsPolishCharacters) return false;

        var containsSpanishCharacters = "áéíúüñÁÉÍÚÜÑ¿¡".Any(query.Contains);
        var containsSpanishCharactersOrWords = containsSpanishCharacters;
        if (!containsSpanishCharactersOrWords)
        {
            containsSpanishCharactersOrWords = ContainsCommonSpanishWords(query);
        }

        return (containsSpanishCharactersOrWords) && !ContainsCommonPolishWords(query);
    }

    private static bool ContainsCommonSpanishWords(string query) =>
        CommonSpanishWords
            .Any(x =>
                query.Contains($" {x} ", StringComparison.InvariantCultureIgnoreCase) ||
                query.StartsWith($"{x} ", StringComparison.InvariantCultureIgnoreCase) ||
                query.EndsWith($" {x}", StringComparison.InvariantCultureIgnoreCase)
            // seems to not add value and gives minor performance gains:
            //||query.EndsWith($" {x}.", StringComparison.InvariantCultureIgnoreCase) ||
            //query.EndsWith($" {x}?", StringComparison.InvariantCultureIgnoreCase) ||
            //query.EndsWith($" {x}!", StringComparison.InvariantCultureIgnoreCase)
            );

    private static bool ContainsCommonPolishWords(string query) =>
        CommonPolishWords
            .Any(x =>
                query.Contains($" {x} ", StringComparison.InvariantCultureIgnoreCase) ||
                query.StartsWith($"{x} ", StringComparison.InvariantCultureIgnoreCase) ||
                query.EndsWith($" {x}", StringComparison.InvariantCultureIgnoreCase)
            // seems to not add value and gives minor performance gains:
            //||query.EndsWith($" {x}.", StringComparison.InvariantCultureIgnoreCase) ||
            //query.EndsWith($" {x}?", StringComparison.InvariantCultureIgnoreCase) ||
            //query.EndsWith($" {x}!", StringComparison.InvariantCultureIgnoreCase)
            );

    public static bool IsStringLikelyInPolishLanguage(string query)
    {
        var containsSpanishCharacters = "áéíúüñÁÉÍÚÜÑ¿¡".Any(query.Contains);
        if (containsSpanishCharacters) return false;

        var containsPolishCharacters = "ąćęłńśźżĄĆĘŁŃŚŻŹ".Any(query.Contains);
        var containsPolishCharactersOrWords = containsPolishCharacters;
        if (!containsPolishCharactersOrWords)
        {
            containsPolishCharactersOrWords = ContainsCommonPolishWords(query);
        }

        return containsPolishCharactersOrWords && !ContainsCommonSpanishWords(query);
    }

    [GeneratedRegex(@"```(.*)```\Z", RegexOptions.Singleline)]
    private static partial Regex BackticksRegex();

    /// <summary>
    ///     Removes the triple backticks and the content type from the string (both at the beginning and at the end of a
    ///     string) if it exists.
    /// 
    ///     Input example:
    ///     ```html
    ///     <p>Content</p>
    ///     ```
    /// 
    ///     Output example:
    ///     <p>Content</p>
    /// </summary>
    public static string RemoveBackticksBlockWrapper(string input)
    {
        // Use regular expression to find matches
        var match = BackticksRegex().Match(input);

        // Return the first group of the match if it's successful

        if (match.Success)
        {
            return match.Groups[1].Value.Trim('`', '\n', '\r', '\t', ' ').Trim();
        }

        return input;
    }


    private static readonly HashSet<Char> exceptions = new HashSet<Char> { 'і', 'І', 'и', 'И' };
    private static readonly HashSet<Char> spanishExceptions = new HashSet<Char> { 'u', 'i' };
    private static readonly HashSet<Char> latinCharacters = new HashSet<Char> { 'e', 'o', 'a', 's', 'l', 'z', 'c', 'n', 'E', 'O', 'A', 'S', 'L', 'Z', 'C', 'N' };

    /// <summary>
    /// Removes accent marks from a string - but only the accent marks I typically see in my Ukrainian flashcards deck; not all the diacritics.
    /// This method is supposed to RETAIN accents in Spanish, Polish and even some ukrainian characters where they are part of the word (like "ї").
    /// 
    /// Generated by ChatGPT, fixed for edge cases:
    /// - ї (we want to keep both dots ;))
    /// - й (we want to keep the character)
    /// </summary>
    public static string RemoveUkrainianFlashcardsAccentMark(this string inputString)
    {
        string normalizedString = inputString.Normalize(NormalizationForm.FormD);
        StringBuilder stringBuilder = new StringBuilder();
        for (int i = 0; i < normalizedString.Length; i++)
        {
            Char c = normalizedString[i];
            Char lastCharacter = i > 0 ? normalizedString[i - 1] : '!';

            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark ||
                latinCharacters.Contains(lastCharacter) ||
                (exceptions.Contains(lastCharacter) && (c == (char)774) || (c == (char)776)) ||
                (spanishExceptions.Contains(lastCharacter) && (c == (char)769))
               )
            {
                stringBuilder.Append(c);
            }
            else
            {
                // character is skipped!
                // good place for a breakpoint
            }
        }
        return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
    }

    /// <summary>
    /// string.GetHashCode() in modern .NET is not stable, i.e. it gives different results after the application is restarted.
    /// I need a stable hash to help me create a cache key for long content like ChatGPT prompts and use as filename, so this helper method helps achieve that.
    /// </summary>
    internal static string GetHashCodeStable(this string input)
    {
        var stableHashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        var stableHash = BitConverter.ToString(stableHashBytes).Replace("-", string.Empty);
        return stableHash;
    }
}
