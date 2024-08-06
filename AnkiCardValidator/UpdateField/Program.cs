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
        var notes = AddPolishTranslationToRemarks.LoadNotesThatRequireAdjustment();
        await AddPolishTranslationToRemarks.AddPolishTranslation(notes);

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
