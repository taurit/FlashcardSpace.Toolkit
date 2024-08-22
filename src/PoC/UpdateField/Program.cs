using AnkiCardValidator;
using AnkiCardValidator.Utilities;
using AnkiCardValidator.ViewModels;
using Spectre.Console;
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
        await new TestGenerativeFill().DoTestGenerativeFill();
        return;

        //var notes = AnkiHelpers.GetNotes(Settings.AnkiDatabaseFilePath, limitToTag: "5-minute-ukrainian").ToList();
        //MoveSoundToSoundField.RunMigration(notes);
        //UpdateNotesInDatabase(notes, userConfirmationRequired: true);

        var notes = MoveImageToImageField.LoadNotesThatRequireAdjustment();

        var chunks = notes.Chunk(30).ToList();
        int chunkNo = 0;
        foreach (var chunk in chunks)
        {
            Console.WriteLine($"Processing chunk {++chunkNo} of {chunks.Count}...");
            var chunkItems = chunk.ToList();
            MoveImageToImageField.RunMigration(chunkItems);
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

