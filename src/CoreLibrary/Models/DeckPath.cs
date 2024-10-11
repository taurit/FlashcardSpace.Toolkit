namespace CoreLibrary.Models;

public record DeckPath
{
    public string DeckOuterPath { get; init; }
    public DeckPath(string deckOuterPath)
    {
        // make sure that it's an absolute path with relative fragments like `/../` resolved
        DeckOuterPath = Path.GetFullPath(deckOuterPath);
    }

    public string DeckDataPath => Path.Combine(DeckOuterPath, "FlashcardDeck");
    public string PreviewIndexHtmlPath => Path.Combine(DeckOuterPath, "index.html");

    public string DeckManifestPath => Path.Combine(DeckDataPath, "flashcards.json");
    public string DeckManifestEditsPath => Path.Combine(DeckDataPath, "flashcards.edited.json");
    public string DeckManifestEditsPathWithFallback => File.Exists(DeckManifestEditsPath) ? DeckManifestEditsPath : DeckManifestPath;
    public string DismissedSuggestionsPath => Path.Combine(DeckDataPath, "dismissed-suggestions.json");

    public string AnkiExportPath => Path.Combine(DeckDataPath, "Output");
    public string AudioProviderCacheFolder => Path.Combine(DeckDataPath, "audio");
    public string TtsCacheFolder => Path.Combine(DeckDataPath, "TTSCache");
    public string GeminiCacheFolder => Path.Combine(DeckDataPath, "GeminiCache");
}
