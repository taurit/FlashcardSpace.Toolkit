namespace AnkiCardValidator.Utilities;
public static class StringHelpers
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
}
