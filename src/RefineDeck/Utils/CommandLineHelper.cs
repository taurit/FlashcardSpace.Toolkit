using System.IO;
using System.Windows;

namespace RefineDeck.Utils;
internal static class CommandLineHelper
{
    public static string GetDeckFolderPath()
    {
        string[] args = Environment.GetCommandLineArgs();

        if (args.Length > 1)
        {
            var launchParameter = args[1];
            var pathToDeck = launchParameter;

            if (launchParameter.StartsWith("refinedeck:///"))
            {
                // redirect comes from the browser (Deck Preview tool has link to edit the previewed deck)
                pathToDeck = launchParameter.Replace("refinedeck:///", "");
                pathToDeck = Path.Combine(pathToDeck, "FlashcardDeck"); // follows convention of deck folder structure
            }

            return pathToDeck;
        }

        MessageBox.Show("Please provide a deck folder path as a command line argument.");

        Application.Current.Shutdown();
        return null!;
    }
}
