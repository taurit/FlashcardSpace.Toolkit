namespace CoreLibrary.Utilities;
public static class StringExtensionMethodsPolish
{
    private static readonly string[] CommonPolishWords = ["nie", "to", "się", "na", "co", "jest", "do", "tak", "jak", "mnie", "za", "ja", "ci", "tu", "go", "tym", "ty", "czy", "tylko", "po", "jestem", "ma", "w"];

    internal static bool ContainsCommonPolishWords(string query) =>
        CommonPolishWords
            .Any(x =>
                query.Contains($" {x} ", StringComparison.InvariantCultureIgnoreCase) ||
                query.StartsWith($"{x} ", StringComparison.InvariantCultureIgnoreCase) ||
                query.EndsWith($" {x}", StringComparison.InvariantCultureIgnoreCase)
            );

    public static bool IsStringLikelyInPolishLanguage(this string query)
    {
        var containsSpanishCharacters = "áéíúüñÁÉÍÚÜÑ¿¡".Any(query.Contains);
        if (containsSpanishCharacters) return false;

        var containsPolishCharacters = "ąćęłńśźżĄĆĘŁŃŚŻŹ".Any(query.Contains);
        var containsPolishCharactersOrWords = containsPolishCharacters;
        if (!containsPolishCharactersOrWords)
        {
            containsPolishCharactersOrWords = ContainsCommonPolishWords(query);
        }

        return containsPolishCharactersOrWords && !StringExtensionMethodsSpanish.ContainsCommonSpanishWords(query);
    }

}
