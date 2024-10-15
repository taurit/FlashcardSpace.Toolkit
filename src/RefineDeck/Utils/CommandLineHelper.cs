using CoreLibrary.Models;
using System.IO;
using System.Reflection;
using System.Windows;

namespace RefineDeck.Utils;

internal static class CommandLineHelper
{
    internal static DeckPath GetDeckFolderPath()
    {
        string[] args = Environment.GetCommandLineArgs();

        // by default, look for deck in the same folder as the executable, in case I delivered *.exe and a deck for review to another person
        string pathCandidate = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        // but if there is a command line argument, use it as the path
        if (args.Length == 2)
            pathCandidate = args[1];

        // support path to input deck in various formats, to allow easy drag&drop to tool's icon without
        // much thinking, including:
        // - d:\DeckWorkspace
        // - d:\DeckWorkspace\index.html
        // - d:\DeckWorkspace\FlashcardDeck
        // - d:\DeckWorkspace\FlashcardDeck\flashcards.json
        // - d:\DeckWorkspace\FlashcardDeck\flashcards.edited.json



        var hypotheticalOuterPaths = new List<DeckPath>()
            {
                new(Path.GetFullPath(pathCandidate)!),
                new(Path.GetDirectoryName(pathCandidate)!),
            };

        // Useful fallback:
        // - d:\{MostRecentlyCreatedDeckInASpecifiedFolder}
        var mostRecentlyModifiedSubfolder = Directory
            .GetDirectories(pathCandidate)
            .Where(x => Directory.Exists(Path.Combine(x, "FlashcardDeck")))
            .OrderByDescending(Directory.GetLastWriteTimeUtc)
            .FirstOrDefault();

        if (mostRecentlyModifiedSubfolder is not null)
            hypotheticalOuterPaths.Add(new DeckPath(mostRecentlyModifiedSubfolder));

        foreach (var hypotheticalOuterPath in hypotheticalOuterPaths)
        {
            var dataPath = Path.Combine(hypotheticalOuterPath.DeckOuterPath, "FlashcardDeck");
            if (Directory.Exists(dataPath))
                return new DeckPath(hypotheticalOuterPath.DeckOuterPath);
        }

        MessageBox.Show("Please provide a valid deck folder path as a command line argument.");
        throw new Exception("Please provide a valid deck folder path as a command line argument.");
    }
}
