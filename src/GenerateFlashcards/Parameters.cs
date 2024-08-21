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

    public static string RootAppDataFolderPath
    {
        get
        {
            const string appName = "GenerateFlashcards";
            string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string cacheFolder = Path.Combine(appDataFolder, appName);

            if (!_alreadyEnsuredAppDataFolderExists)
            {
                Directory.CreateDirectory(cacheFolder);
                _alreadyEnsuredAppDataFolderExists = true;
            }

            return cacheFolder;
        }
    }
    private static bool _alreadyEnsuredAppDataFolderExists = false;

}
