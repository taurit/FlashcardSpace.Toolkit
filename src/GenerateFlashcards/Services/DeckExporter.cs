using GenerateFlashcards.Models;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GenerateFlashcards.Services;

/// <summary>
/// Exports generated deck of flashcards into a folder containing:
/// - `flashcards.json` file describing the deck and all the flashcards it contains
/// - `audio` folder containing all the audio files for the flashcards
/// - `images` folder containing all the images for the flashcards
/// </summary>
internal class DeckExporter(string browserProfileDirectory)
{
    public void ExportDeck(Deck deck, string exportFolderPath)
    {
        // the name of subfolder from which flashcards are loaded; hardcoded in web app, must be the same here
        const string flashcardDeckSubfolder = "FlashcardDeck";

        var deckSubfolder = Path.Combine(exportFolderPath, flashcardDeckSubfolder);
        Directory.CreateDirectory(deckSubfolder);
        var metadataFile = Path.Combine(deckSubfolder, "flashcards.json");

        // Export the deck description
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() } // Add JsonStringEnumConverter to serialize enums as strings
        };
        var deckSerialized = JsonSerializer.Serialize(deck, options);
        File.WriteAllText(metadataFile, deckSerialized);

        // Export the audio files
        var audioFolderPath = Path.Combine(deckSubfolder, "audio");
        Directory.CreateDirectory(audioFolderPath);
        foreach (var flashcard in deck.Flashcards)
        {
            //var audioFilePath = Path.Combine(audioFolderPath, $"{flashcard.Id}.mp3");
            //File.WriteAllBytes(audioFilePath, flashcard.Audio);
        }

        // Export the images
        var imagesFolderPath = Path.Combine(deckSubfolder, "images");
        Directory.CreateDirectory(imagesFolderPath);
        foreach (var flashcard in deck.Flashcards)
        {
            //var imageFilePath = Path.Combine(imagesFolderPath, $"{flashcard.Id}.png");
            //File.WriteAllBytes(imageFilePath, flashcard.Image);
        }

        // copy the JS app that allows to browse the deck.
        var previewAppDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "Resources", "PreviewApp");
        var previewAppFile = Path.Combine(previewAppDirectory, "index.html");
        var previewAppDestination = Path.Combine(exportFolderPath, "index.html");
        File.Copy(previewAppFile, previewAppDestination);
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
                        $"--user-data-dir=\"{browserProfileDirectory}\" " +
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

