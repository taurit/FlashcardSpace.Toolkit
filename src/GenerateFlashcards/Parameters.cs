namespace GenerateFlashcards;

/// <summary>
/// Global parameters for running the application which are outside of user's control.
/// 
/// Many of those values are needed in the Dependency Injection setup phase, so if we wanted them accepted as command-line arguments
/// as everything else, it would require some secondary mechanism (besides Spectre.Console.Cli) to parse `string[] args` from
/// the `Main` method. That would not be elegant and I cannot see better solution yet.
/// </summary>
internal static class Parameters
{
    // Flagship models are listed at: https://platform.openai.com/docs/models
    // Use cheap model in development but the best one to generate the final output
    public const string OpenAiModelId = "gpt-4o-2024-08-06";

    /// Arbitrary identifier of model's class, used as a key when caching responses. For example, if we want cache outputs
    /// generated with `gpt-4o-preview` to remain utilized after upgrade to `gpt-4o`, just use the same value here.
    public const string OpenAiModelClassId = "gpt-4o";

    private static readonly string RootAppDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FlashcardSpaceToolkitCaches");
    internal static readonly string ChatGptClientCacheFolder = Path.Combine(RootAppDataFolder, "GenerateFlashcards.ChatGptClient");
    internal static readonly string GenerativeFillCacheFolder = Path.Combine(RootAppDataFolder, "GenerateFlashcards.GenerativeFill");
    internal static readonly string TextToSpeechCacheFolder = Path.Combine(RootAppDataFolder, "GenerateFlashcards.TextToSpeech");
    internal static readonly string BrowserProfileDirectory = Path.Combine(RootAppDataFolder, "GenerateFlashcards.DeckExporter.EdgeProfile");
    internal static readonly string ImageGeneratorCacheFolder = Path.Combine(RootAppDataFolder, "GenerateFlashcards.ImageGenerator");
    internal static readonly string ImageProviderCacheFolder = Path.Combine(RootAppDataFolder, "GenerateFlashcards.ImageProvider");
    internal static readonly string AudioCacheFolder = Path.Combine(RootAppDataFolder, "GenerateFlashcards.Audio");
    internal static readonly string DeckExportPath = Path.Combine(RootAppDataFolder, "GenerateFlashcards.Outputs");
}
