namespace CoreLibrary.Utilities;
public static class StringExtensionMethodsSpanish
{
    // https://en.wikipedia.org/wiki/Most_common_words_in_Spanish + own experience of words common in language learning like nuestro
    private static readonly string[] CommonSpanishWords = ["de", "la", "se", "los", "las", "con", "el", "en", "le", "que", "y", "del", "un", "por", "con", "una", "su", "para", "es", "al", "lo", "como", "pero", "mi", "si", "me", "fue", "era", "han", "hay", "yo", "nuestro", "vuestro", "vosotros", "nosotros", "ellos", "ella", "te"];
    internal static bool ContainsCommonSpanishWords(string query) =>
        CommonSpanishWords
            .Any(x =>
                query.Contains($" {x} ", StringComparison.InvariantCultureIgnoreCase) ||
                query.StartsWith($"{x} ", StringComparison.InvariantCultureIgnoreCase) ||
                query.EndsWith($" {x}", StringComparison.InvariantCultureIgnoreCase)
            );

    public static bool IsStringLikelyInSpanishLanguage(this string query)
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

        return containsSpanishCharactersOrWords && !StringExtensionMethodsPolish.ContainsCommonPolishWords(query);
    }



}
