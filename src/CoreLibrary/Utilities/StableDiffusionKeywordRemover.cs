namespace CoreLibrary.Utilities;

[Obsolete("Unused, to be removed after November 1, 2024 unless I need it until that time")]
public static class StableDiffusionKeywordRemover
{
    /// <summary>
    ///  By convention, the last 3 keywords in prompt are just a random descriptors of the style (e.g. "painting, gloomy, 1980s"). It's useful to remove them to get a more general prompt that we can compare with other prompts for similar meaning, skipping the style part.
    /// </summary>
    /// <example>
    /// For prompt:
    /// She is dressed in blue.,dressed,drone shot,minimalist,desaturated
    ///
    /// This method should return:
    /// She is dressed in blue.,dressed
    /// </example>
    public static string RemoveStyleKeywordsFromPrompt(string fullPrompt)
    {
        if (String.IsNullOrWhiteSpace(fullPrompt)) return fullPrompt;

        // find position of 3rd comma from the end
        var lastCommaIndex = fullPrompt.LastIndexOf(',');
        if (lastCommaIndex == -1) return fullPrompt;

        lastCommaIndex = fullPrompt.LastIndexOf(',', lastCommaIndex - 1);
        if (lastCommaIndex == -1) return fullPrompt;

        lastCommaIndex = fullPrompt.LastIndexOf(',', lastCommaIndex - 1);
        if (lastCommaIndex == -1) return fullPrompt;

        return fullPrompt.Substring(0, lastCommaIndex);
    }
}
