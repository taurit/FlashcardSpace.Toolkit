using CoreLibrary.Services;

namespace Anki.Net.TestApp;

internal class Program
{
    static void Main(string[] args)
    {
        var ankiExportService = new AnkiExportService();
        ankiExportService.ExportToAnki(@"d:\Projekty\FlashcardSpace.Toolkit\src\DeckBrowser\FlashcardDeck\", @"d:\TestDeck");
    }
}
