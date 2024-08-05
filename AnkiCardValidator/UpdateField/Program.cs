using AnkiCardValidator;
using AnkiCardValidator.Utilities;
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

        // Debug: inspect note (display all fields); use Spectre.Console package to display it in a nice table
        var firstNote = notesToUpdate.First();
        UiHelper.DisplayAnkiNote(firstNote);

        // Simulate: update the Examples field
        AnkiHelpers.UpdateFields(Settings.AnkiDatabaseFilePath, notes);

    }
}
