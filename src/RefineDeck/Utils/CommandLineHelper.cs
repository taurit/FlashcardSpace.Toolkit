using CoreLibrary.Models;
using System.IO;

namespace RefineDeck.Utils;

internal static class CommandLineHelper
{
    internal static DeckPath GetDeckFolderPath()
    {
        string[] args = Environment.GetCommandLineArgs();

        if (args.Length == 2)
        {
            var launchParameter = args[1];

            // support path to input deck in various formats, to allow easy drag&drop to tool's icon without
            // much thinking, including:
            // - d:\DeckWorkspace
            // - d:\DeckWorkspace\index.html
            // - d:\DeckWorkspace\FlashcardDeck
            // - d:\DeckWorkspace\FlashcardDeck\flashcards.json
            // - d:\DeckWorkspace\FlashcardDeck\flashcards.edited.json

            var hypotheticalOuterPaths = new List<DeckPath>()
            {
                new(Path.GetDirectoryName(launchParameter)!),
                new(Path.Combine(Path.GetDirectoryName(launchParameter)!, "..")),
            };

            foreach (var hypotheticalOuterPath in hypotheticalOuterPaths)
            {
                var dataPath = Path.Combine(hypotheticalOuterPath.DeckOuterPath, "FlashcardDeck");
                if (Directory.Exists(dataPath))
                    return new DeckPath(hypotheticalOuterPath.DeckOuterPath);
            }
        }

        throw new Exception("Please provide a valid deck folder path as a command line argument.");
    }
}
