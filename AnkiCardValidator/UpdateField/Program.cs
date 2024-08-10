using AnkiCardValidator;
using AnkiCardValidator.Utilities;
using AnkiCardValidator.ViewModels;
using Spectre.Console;
using System.Globalization;
using UpdateField.Mutations;
using UpdateField.Utilities;

namespace UpdateField;

internal class Program
{
    /// <summary>
    /// A tool (maybe for one-time use) to batch modify Anki note fields outside Anki context
    /// </summary>
    /// <param name="args"></param>
    static async Task Main(string[] args)
    {
        // Variant 1
        Console.WriteLine("Strings without normalization:");
        var table = new Table();
        table.AddColumns("Field", "Value");
        table.AddRow("Row 1", "ABCDEF");
        table.AddRow("Row 2", "ąęúłśż");
        table.AddRow("Row 3", "áéúíüñ");
        table.AddRow("Row 4", "абвцде");
        table.AddRow("Row 5", "а\u0301б\u0301в\u0301ц\u0301д\u0301е\u0301");
        table.AddRow("Row 6", "a\u0301b\u0301c\u0301d\u0301e\u0301f\u0301");
        table.AddRow("Row 7", "👍👎👌👏👋👊");
        table.AddRow("Row 8", "你好嗎？我很");
        table.AddRow("Row 9", "🇵🇱🇧🇷🇨🇦🇺🇸🇬🇧🇦🇺");
        table.AddRow("Row 10", "أبجد ه");
        AnsiConsole.Write(table);

        // Variant 2
        Console.WriteLine("Normalized with `string.Normalize(NormalizationForm.FormC)`:");
        var table2 = new Table();
        table2.AddColumns("Field", "Value");
        table2.AddRow("Row 1", "ABCDEF".Normalize());
        table2.AddRow("Row 5", "а\u0301б\u0301в\u0301ц\u0301д\u0301е\u0301".Normalize());
        table2.AddRow("Row 6", "a\u0301b\u0301c\u0301d\u0301e\u0301f\u0301".Normalize());
        AnsiConsole.Write(table2);

        // Variant 3
        Console.WriteLine("Normalized with `string.Normalize(NormalizationForm.FormC)`, remaining accent characters removed:");
        string RemoveAccentMarks(string input) => string.Concat(input.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark));
        var table3 = new Table();
        table3.AddColumns("Field", "Value");
        table3.AddRow("Row 1", "ABCDEF".Normalize());
        table3.AddRow("Row 5", RemoveAccentMarks("а\u0301б\u0301в\u0301ц\u0301д\u0301е\u0301".Normalize()));
        table3.AddRow("Row 6", RemoveAccentMarks("a\u0301b\u0301c\u0301d\u0301e\u0301f\u0301".Normalize()));
        AnsiConsole.Write(table3);

        return;

        await new TestGenerativeFill().DoTestGenerativeFill();
        return;

        //var notes = AnkiHelpers.GetNotes(Settings.AnkiDatabaseFilePath, limitToTag: "5-minute-ukrainian").ToList();
        //MoveSoundToSoundField.RunMigration(notes);
        //UpdateNotesInDatabase(notes, userConfirmationRequired: true);

        var notes = AddPolishTranslationToRemarks.LoadNotesThatRequireAdjustment();

        var chunks = notes.Chunk(30).ToList();
        int chunkNo = 0;
        foreach (var chunk in chunks)
        {
            Console.WriteLine($"Processing chunk {++chunkNo} of {chunks.Count}...");
            var chunkItems = chunk.ToList();
            await AddPolishTranslationToRemarks.AddPolishTranslation(chunkItems);
            UpdateNotesInDatabase(chunkItems, userConfirmationRequired: false);

        }

    }

    private static void UpdateNotesInDatabase(List<AnkiNote> notes, bool userConfirmationRequired = true)
    {
        var modifiedNotes = notes.Where(x => x.FieldsRawCurrent != x.FieldsRawOriginal).ToList();
        UiHelper.DisplayModifiedNotesDiff(modifiedNotes);

        if (modifiedNotes.Count == 0) return;
        if (!userConfirmationRequired ||
            AnsiConsole.Confirm($"Do you want to perform the modification on a real database [red]({Settings.AnkiDatabaseFilePath})[/]?", false))
        {
            AnkiHelpers.UpdateFields(Settings.AnkiDatabaseFilePath, notes);
        }
    }
}

