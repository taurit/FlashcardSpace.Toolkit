using AnkiCardValidator;
using AnkiCardValidator.Utilities;
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
        const string remarkId = "pl";

        // Load notes that need adjustment
        var notes = AnkiHelpers.GetAllNotesFromSpecificDeck(Settings.AnkiDatabaseFilePath, "2. Ukrainian", "addSmartExampleUkr")
                .Where(x => !x.Remarks.HasRemark(remarkId))
                .Take(10) // debug
                .ToList();
        ;

        // ACTION: perform mutations on the flashcard notes
        await AddPolishTranslationToRemarks.AddPolishTranslation(notes, remarkId);

        // Display modified notes to allow user confirm/reject changes
        var modifiedNotes = notes.Where(x => x.FieldsRawCurrent != x.FieldsRawOriginal).ToList();
        UiHelper.DisplayModifiedNotesDiff(modifiedNotes);

        if (modifiedNotes.Count == 0) return;
        if (AnsiConsole.Confirm($"Do you want to perform the modification on a real database [red]({Settings.AnkiDatabaseFilePath})[/]?", false))
        {
            AnkiHelpers.UpdateFields(Settings.AnkiDatabaseFilePath, notes);
        }

    }
}
