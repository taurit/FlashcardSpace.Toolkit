using AnkiNet;
using AnkiNet.Models;
using AnkiNet.Models.Scriban;

namespace Anki.Net.TestApp;

internal class Program
{
    static void Main(string[] args)
    {
        var fields = new FieldList {
            new Field("English"),
            new Field("Ukrainian"),
            new Field("ExampleA"),
        };

        var cardTemplates = new CardTemplate[] {
            new CardTemplate(0, "Forward", "{{English}}", "{{FrontSide}}\n<hr id=answer>{{Ukrainian}}\n{{ExampleA}}")
        };

        var ankiNoteModel = new AnkiDeckModel("Test deck with 3 words", fields, cardTemplates, "tst");
        var deck = new AnkiDeck(ankiNoteModel);

        var fileName1 = deck.RegisterAudioFile("D:\\Flashcards\\.Cache\\AudioSentences\\ви_знаєте_хто_його_прислав.mp3");
        var fileName2 = deck.RegisterImageFile("d:\\Midjourney\\Promo pics\\adeyemichane_daniel_radcliffe_and_Ron_Weasley_drinking_shots_705e5b3b-68ee-4cdc-a7c5-fdab73dab413.png");

        deck.AddItem($"EnglishWord<img src=\"{fileName2}\" />", "UkrainianWord", "Example 1");
        deck.AddItem("Field in language 3a", $"Field in language 3a<br />[sound:{fileName1}]", "Example 3");

        deck.CreateApkgFile(@"d:\TestDeck");
    }
}
