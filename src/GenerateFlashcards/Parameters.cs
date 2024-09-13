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
    public const string DocumentationUrlAboutUserSecrets = "https://github.com/taurit/FlashcardSpace.Toolkit/blob/main/docs/Secrets.md";

    // Flagship models are listed at: https://platform.openai.com/docs/models
    // Use cheap model in development but the best one to generate the final output
    public const string OpenAiModelId = "gpt-4o-mini";

    /// Arbitrary identifier of model's class, used as a key when caching responses. For example, if we want cache outputs
    /// generated with `gpt-4o-preview` to remain utilized after upgrade to `gpt-4o`, just use the same value here.
    public const string OpenAiModelClassId = "gpt-mini";

    private static Lazy<string> RootAppDataFolder => new(() => EnsureSubfolderExists(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FlashcardSpaceToolkitCaches"));
    internal static Lazy<string> ChatGptClientCacheFolder => new(() => EnsureSubfolderExists(RootAppDataFolder.Value, "GenerateFlashcards.ChatGptClient"));
    internal static Lazy<string> GenerativeFillCacheFolder => new(() => EnsureSubfolderExists(RootAppDataFolder.Value, "GenerateFlashcards.GenerativeFill"));

    private static string EnsureSubfolderExists(string rootPath, string subfolder)
    {
        var cacheFolder = Path.Combine(rootPath, subfolder);
        Directory.CreateDirectory(cacheFolder);
        return cacheFolder;
    }
}
