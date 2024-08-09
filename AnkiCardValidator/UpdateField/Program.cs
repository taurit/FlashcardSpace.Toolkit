using AnkiCardValidator;
using AnkiCardValidator.Utilities;
using AnkiCardValidator.Utilities.JsonGenerativeFill;
using AnkiCardValidator.ViewModels;
using Spectre.Console;
using System.Diagnostics;
using UpdateField.Mutations;
using UpdateField.Utilities;

namespace UpdateField;

internal class Program
{
    [DebuggerDisplay("{Country} capital is {Capital}")]
    class CompleteCapitalJob(string country) : ItemWithId
    {
        public string Country { get; set; } = country;

        [Fill]
        public string? Capital { get; set; }
    }

    /// <summary>
    /// A tool (maybe for one-time use) to batch modify Anki note fields outside Anki context
    /// </summary>
    /// <param name="args"></param>
    static async Task Main(string[] args)
    {
        var response = await JsonGenerativeFill.GetAnswersToPromptsUsingChatGptApi(
            "- Output should contain the name of the capital of the given country.",
            [
                new CompleteCapitalJob("Poland"),
                new CompleteCapitalJob("Ukraine"),
                new CompleteCapitalJob("Germany"),
            ]);

        foreach (var job in response)
        {
            Console.WriteLine($"{job.Country} capital is {job.Capital}");
        }


        return;

        var gemini = new GeminiHelper();
        var answer = await gemini.GetAnswer(
            "Given input in Ukrainian and usage context in English, provide the closest equivalent in Polish. Be as accurate as possible.\r\nEnglish translation or comment is provided to clarify the context.\r\nIf you encounter Anki tags like <img/> or [sound:...], ignore them as if they weren't in the input.\r\n\r\nThe input is:\r\n\r\n```json\r\n{\r\n  \"Flashcards\": [\r\n    {\r\n      \"Id\": 1,\r\n      \"Ukrainian\": \"розважа\u0301ти, розва\u0301жити<br>[sound:87 розважати розважити.mp3]\",\r\n      \"English\": \"to entertain sb (imperfective, perfective)\"\r\n    },\r\n    {\r\n      \"Id\": 2,\r\n      \"Ukrainian\": \"наро\u0301джуватись, народи\u0301тись<br />[sound:81 народжуватись народитись.mp3]\",\r\n      \"English\": \"to be born (imperfective, perfective)\"\r\n    },\r\n    {\r\n      \"Id\": 3,\r\n      \"Ukrainian\": \"збі\u0301рка\",\r\n      \"English\": \"compilation, book (usually poetry)\"\r\n    },\r\n    {\r\n      \"Id\": 4,\r\n      \"Ukrainian\": \"висипа\u0301тись, ви\u0301спатись\",\r\n      \"English\": \"to get enough sleep (imperfective, perfective)\"\r\n    },\r\n    {\r\n      \"Id\": 5,\r\n      \"Ukrainian\": \"обіця\u0301ти (недок.) – пообіця\u0301ти (док.)[sound:rec1545424237.mp3]\",\r\n      \"English\": \"obiecywać, obiecać\"\r\n    },\r\n    {\r\n      \"Id\": 6,\r\n      \"Ukrainian\": \"як гаря\u0301чі пиріжки<br />[sound:rec1545424679.mp3]\",\r\n      \"English\": \"lit. “like hot buns”. to be sold or given away very quickly and effortlessly, especially in quantity, like hot cakes\"\r\n    },\r\n    {\r\n      \"Id\": 7,\r\n      \"Ukrainian\": \"я\u0301гідний[sound:rec1545424664.mp3]\",\r\n      \"English\": \"\"\r\n    },\r\n    {\r\n      \"Id\": 8,\r\n      \"Ukrainian\": \"пампу\u0301х<br />[sound:rec1545424254.mp3]\",\r\n      \"English\": \"a small savory or sweet bun or doughnut typical for Ukrainian cuisine\"\r\n    },\r\n    {\r\n      \"Id\": 9,\r\n      \"Ukrainian\": \"розклада\u0301ти по поли\u0301чках, розкла\u0301сти по поли\u0301чках\",\r\n      \"English\": \"\\\"to put out on shelves\\\": 1) to break sth down, to make things clear<br />2) to compartmentalize (imperfective, perfective)\"\r\n    },\r\n    {\r\n      \"Id\": 10,\r\n      \"Ukrainian\": \"наїда\u0301тися, наї\u0301стися\",\r\n      \"English\": \"to eat one's fill, to eat to satiety (to repletion) (imperfective, perfective)\"\r\n    }\r\n  ]\r\n}\r\n\r\n```\r\n\r\nThe output should follow the following format, and fill the null values with the correct Polish translation:\r\n\r\n```json\r\n{\r\n  \"Flashcards\": [\r\n    {\r\n      \"Id\": 1,\r\n      \"Polish\": null\r\n    },\r\n    ...\r\n  ]\r\n}\r\n```");
        Console.WriteLine(answer);
        await File.WriteAllTextAsync("d:/gemini-via-api.json", answer);
        return;

        //var notes = AddPolishTranslationToRemarks.LoadNotesThatRequireAdjustment();
        //await AddPolishTranslationToRemarks.AddPolishTranslation(notes);

        var notes = MoveImageToImageField.LoadNotesThatRequireAdjustment();
        MoveImageToImageField.RunMigration(notes);

        // Display modified notes to allow user confirm/reject changes (should work for all types of mutations!)
        ConfirmAndUpdateNotesInDatabase(notes);
    }

    private static void ConfirmAndUpdateNotesInDatabase(List<AnkiNote> notes)
    {
        var modifiedNotes = notes.Where(x => x.FieldsRawCurrent != x.FieldsRawOriginal).ToList();
        UiHelper.DisplayModifiedNotesDiff(modifiedNotes);

        if (modifiedNotes.Count == 0) return;
        if (AnsiConsole.Confirm($"Do you want to perform the modification on a real database [red]({Settings.AnkiDatabaseFilePath})[/]?", false))
        {
            AnkiHelpers.UpdateFields(Settings.AnkiDatabaseFilePath, notes);
        }
    }
}

