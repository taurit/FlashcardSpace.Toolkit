using AnkiCardValidator;
using AnkiCardValidator.Utilities;
using AnkiCardValidator.ViewModels;
using UpdateField.Utilities;

namespace UpdateField.Mutations;
public static class RemoveRedundantWrapperDiv
{

    public static List<AnkiNote> LoadNotesThatRequireAdjustment() =>
        AnkiHelpers.GetNotes(Settings.AnkiDatabaseFilePath, limitToTag: "s05")
            .ToList();

    public static void RunMigration(List<AnkiNote> notes)
    {
        foreach (var note in notes)
        {
            RemoveWrapperDivIfNeeded(note);
        }
    }

    /// <summary>
    /// If a given note has an image in the FrontText field or BackText field, move it to the Image field.
    /// </summary>
    public static void RemoveWrapperDivIfNeeded(AnkiNote note)
    {
        // Use HTMLAgilityPack to detect if there's an image in the FrontText or BackText field
        MutationHelpers.RemoveWrapperDivs(note);

    }
}
