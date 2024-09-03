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
    public const string UrlToDocumentationAboutDefiningUserSecrets = "https://github.com/taurit/FlashcardSpace.Toolkit/blob/main/docs/Secrets.md";
    private const string AppName = "FlashcardSpaceToolkitCaches";

    // Flagship models are listed at: https://platform.openai.com/docs/models

    // For development, I use small and cheap models like `gpt-4o-mini` which are good enough to produce responses
    // conforming to the schema and usually being correct.
    // To generate real flashcard sets however, we want to switch to state-of-the-art models optimized for best intelligence.
    public const string OpenAiModelId = "gpt-4o-mini";

    /// Arbitrary identifier of model's class, used as a key when caching responses. For example, if we want cache outputs
    /// generated with `gpt-4o-preview` to remain utilized after upgrade to `gpt-4o`, just use the same value here.
    public const string OpenAiModelClassId = "gpt-mini";

    private static Lazy<string> RootAppDataFolder => new(() =>
    {
        var appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var cacheFolder = Path.Combine(appDataFolder, AppName);

        Directory.CreateDirectory(cacheFolder);

        return cacheFolder;
    });

    internal static Lazy<string> ChatGptClientCacheFolder => new(() =>
    {
        var cacheFolder = Path.Combine(RootAppDataFolder.Value, "GenerateFlashcards.ChatGptClient");
        Directory.CreateDirectory(cacheFolder);
        return cacheFolder;
    });

    internal static Lazy<string> GenerativeFillCacheFolder => new(() =>
    {
        var cacheFolder = Path.Combine(RootAppDataFolder.Value, "GenerateFlashcards.GenerativeFill");
        Directory.CreateDirectory(cacheFolder);
        return cacheFolder;
    });
}
