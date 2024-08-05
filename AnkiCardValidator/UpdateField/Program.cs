using AnkiCardValidator;
using AnkiCardValidator.Utilities;
using Spectre.Console;
using UpdateField.Utilities;

namespace UpdateField;

internal class Program
{
    /// <summary>
    /// A quick tool, maybe for one-time use, to generate field value for selected subset of Anki cards using a prompt and other field values.
    /// </summary>
    /// <param name="args"></param>
    static void Main(string[] args)
    {
        // Load notes that need adjustment
        var notes = AnkiHelpers.GetAllNotesFromSpecificDeck(Settings.AnkiDatabaseFilePath, "2. Ukrainian");
        var notesToUpdate = notes.Where(x => String.IsNullOrWhiteSpace(x.Comments) && x.Tags.Contains("addSmartExampleUkr")).ToList();

        // Test: some arbitrary rule to test the diff UI for updates
        foreach (var note in notesToUpdate)
        {
            if (note.BackText.Contains(" "))
            {
                note.BackText = note.BackText.Replace(" ", "&nbsp;");
            }
        }

        // Debug: inspect note (display all fields); use Spectre.Console package to display it in a nice table
        var modifiedNotes = notesToUpdate.Where(x => x.FieldsRawCurrent != x.FieldsRawOriginal).ToList();

        AnsiConsole.MarkupLine($"[aqua]{modifiedNotes.Count} notes are scheduled for modification:[/]");
        if (modifiedNotes.Count == 0) return;

        foreach (var modifiedNote in modifiedNotes)
        {
            UiHelper.DisplayAnkiNote(modifiedNote);
        }

        if (AnsiConsole.Confirm($"Do you want to perform the modification on a real database [red]({Settings.AnkiDatabaseFilePath})[/]?"))
        {
            //AnkiHelpers.UpdateFields(Settings.AnkiDatabaseFilePath, notes);
        }

    }
}
