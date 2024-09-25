using CoreLibrary.Models;
using CoreLibrary.Services.AnkiExportService;

namespace Anki.Net.TestApp;

internal class Program
{
    static void Main(string[] args)
    {
        var ankiExportService = new AnkiExportService();
        ankiExportService.ExportToAnki(new DeckPath(@"c:\Users\windo\AppData\Local\FlashcardSpaceToolkitCaches\GenerateFlashcards.Outputs\Deck-2024-09-25_My_Spanish_lessons\"));
    }
}
