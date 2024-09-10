using AnkiCardValidator;
using AnkiCardValidator.Utilities;
using AnkiCardValidator.ViewModels;

namespace UpdateField.Mutations;

public static class MoveTranslationFromRemarksToFrontText
{

    public static List<AnkiNote> LoadNotesThatRequireAdjustment() =>
        AnkiHelpers.GetNotes(Settings.AnkiDatabaseFilePath, deckName: "2. Ukrainian")
            //.Take(30) // debug
            .ToList();

    public static async Task RunMigration(List<AnkiNote> notes)
    {
        foreach (var note in notes)
        {
            await MigrateField(note);
        }
    }

    /// <summary>
    /// If a given note has an image in the FrontText field or BackText field, move it to the Image field.
    /// </summary>
    public static async Task MigrateField(AnkiNote note)
    {
        var test = note.Remarks.TryGetRemark("pl");
        if (test is not null)
            throw new InvalidOperationException("Inconsistency: this note should have been migrated already.");

        //var translation = note.BackText.TryGetRemark("pl");
        //if (translation == null)
        //    return;

        //var altLangDef = note.BackText.RemoveRemark("pl").Trim();
        //note.BackText = $"{translation}\n<div class=\"alt-lang-def\">{altLangDef}</div>";
    }
}
