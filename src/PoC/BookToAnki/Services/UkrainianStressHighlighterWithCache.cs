using BookToAnki.Interfaces;

namespace BookToAnki.Services;

public class UkrainianStressHighlighterWithCache : PersistedCacheHostJson<Dictionary<string, string>>, IUkrainianStressHighlighter
{
    private readonly UkrainianStressHighlighter _highlighter;
    public Dictionary<string, string> CacheForTestPurposes => this.Cache;

    public UkrainianStressHighlighterWithCache(string cacheFilePath) : base(cacheFilePath)
    {
        _highlighter = new UkrainianStressHighlighter();
    }

    public bool AreStressesCachedAlready(string inputText)
    {
        return Cache.ContainsKey(inputText);
    }

    public async Task<string?> HighlightStresses(string inputText)
    {
        if (Cache.TryGetValue(inputText, out var stresses))
        {
            stresses = FixHighlightsIncorrectlyMarkedBy3RdPartyService(stresses);
            return stresses;
        }

        var result = await _highlighter.HighlightStresses(inputText);
        if (result != null)
        {
            Cache[inputText] = result;
            result = FixHighlightsIncorrectlyMarkedBy3RdPartyService(result);
        }
        return result;
    }

    private static string FixHighlightsIncorrectlyMarkedBy3RdPartyService(string result)
    {
        return result
            .Replace("Гарр<span style=\"color:red\">і</span>", "Г<span style=\"color:red\">а</span>ррі")
            ;
    }
}
