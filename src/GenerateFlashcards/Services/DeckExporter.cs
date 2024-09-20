using CoreLibrary.Models;
using CoreLibrary.Utilities;
using System.Diagnostics;
using System.Reflection;

namespace GenerateFlashcards.Services;

internal record DeckExporterSettings(string BrowserProfileDirectory, string DeckExportPath);

/// <summary>
/// Exports generated deck of flashcards into a folder containing:
/// - `flashcards.json` file describing the deck and all the flashcards it contains
/// - `audio` folder containing all the audio files for the flashcards
/// - `images` folder containing all the images for the flashcards
/// </summary>
internal class DeckExporter(DeckExporterSettings settings)
{
    public void ExportToFolderAndOpenPreview(Deck deck)
    {
        // name folder in the format like `Deck-2024-09-20_08-38_deckName
        var deckNameWithoutSpaces = deck.DeckName.ToFilenameFriendlyString(20).Replace(" ", "_");
        var deckSubfolderName = $"Deck-{DateTime.Now:yyyy-MM-dd}_{deckNameWithoutSpaces}"; // _HH-mm
        var exportDirectory = Path.Combine(settings.DeckExportPath, deckSubfolderName);
        Directory.CreateDirectory(exportDirectory);
        ExportDeck(deck, exportDirectory);

        OpenPreview(exportDirectory);
    }

    public void ExportDeck(Deck deck, string exportFolderPath)
    {
        // the name of subfolder from which flashcards are loaded; hardcoded in web app, must be the same here
        const string flashcardDeckSubfolder = "FlashcardDeck";

        var deckSubfolder = Path.Combine(exportFolderPath, flashcardDeckSubfolder);
        Directory.CreateDirectory(deckSubfolder);
        var metadataFile = Path.Combine(deckSubfolder, "flashcards.json");

        // Export the images
        var imagesFolderPath = Path.Combine(deckSubfolder, "images");
        var audioFolderPath = Path.Combine(deckSubfolder, "audio");
        imagesFolderPath.EnsureDirectoryExists();
        audioFolderPath.EnsureDirectoryExists();

        foreach (var flashcard in deck.Flashcards)
        {
            // copy images to target directory
            foreach (var sourceImagePath in flashcard.ImageCandidates)
            {
                var targetImagePath = Path.Combine(imagesFolderPath, new FileInfo(sourceImagePath).Name);
                if (File.Exists(targetImagePath))
                    continue;
                File.Copy(sourceImagePath, targetImagePath, false);
            }

            // update the image paths to relative paths
            for (var i = 0; i < flashcard.ImageCandidates.Count; i++)
            {
                var imageFileName = new FileInfo(flashcard.ImageCandidates[i]).Name;
                flashcard.ImageCandidates[i] = Path.Combine("images", imageFileName);
            }

            // copy audio files to target directory
            flashcard.TermAudio = CopyAudioFileToDeckAndReturnRelativePath(flashcard.TermAudio, audioFolderPath);
            flashcard.TermTranslationAudio = CopyAudioFileToDeckAndReturnRelativePath(flashcard.TermTranslationAudio, audioFolderPath);
            flashcard.ContextAudio = CopyAudioFileToDeckAndReturnRelativePath(flashcard.ContextAudio, audioFolderPath);
            flashcard.ContextTranslationAudio = CopyAudioFileToDeckAndReturnRelativePath(flashcard.ContextTranslationAudio, audioFolderPath);
        }

        // Export the deck description
        var deckSerialized = deck.Serialize();
        File.WriteAllText(metadataFile, deckSerialized);

        // copy the JS app that allows to browse the deck.
        var previewAppDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "Resources", "PreviewApp");
        var previewAppFile = Path.Combine(previewAppDirectory, "index.html");
        var previewAppDestination = Path.Combine(exportFolderPath, "index.html");
        File.Copy(previewAppFile, previewAppDestination, true);
    }

    private string CopyAudioFileToDeckAndReturnRelativePath(string audioFileFullPath, string targetDirectory)
    {
        // copy file to target directory
        var sourceFileName = new FileInfo(audioFileFullPath).Name;
        var targetFileAbsolutePath = Path.Combine(targetDirectory, sourceFileName);
        var targetFileRelativePath = Path.Combine("audio", sourceFileName);

        if (File.Exists(targetFileAbsolutePath))
        {
            return targetFileRelativePath;
        }
        File.Copy(audioFileFullPath, targetFileAbsolutePath, false);

        return targetFileRelativePath;
    }

    public void OpenPreview(string singleUseExportDirectory)
    {
        // open Edge with:
        // - arguments that disable web security (to allow fetching local file) 
        // - argument pointing to the HTML file named "index.html" in the export directory
        var processStartInfo = new ProcessStartInfo("msedge.exe")
        {
            Arguments = $"--disable-web-security " +
                        // needed to allow the browser to access the local file system
                        // annoying when launched for the first time because there is "first run" wizard and all addons open their tabs
                        $"--user-data-dir=\"{settings.BrowserProfileDirectory}\" " +
                        "--no-first-run " +
                        "--disable-sync " +

                        // Guest mode could be used to avoid showing user's bookmarks
                        // but then it does't remember DevTools settings and is annoying.
                        // keep it disabled during intensive development.
                        //"--guest " +

                        $"\"{Path.Combine(singleUseExportDirectory, "index.html")}\"",
            UseShellExecute = true
        };
        Process.Start(processStartInfo);
    }
}

