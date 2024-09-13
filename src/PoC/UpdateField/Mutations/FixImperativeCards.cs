using AnkiCardValidator;
using AnkiCardValidator.Utilities;
using AnkiCardValidator.ViewModels;

namespace UpdateField.Mutations;

public class FixImperativeCards : IMutation
{

    public List<AnkiNote> LoadNotesThatRequireAdjustment() =>
        AnkiHelpers.GetNotes(Settings.AnkiDatabaseFilePath, limitToTag: "imp-aff-pl")
            //.Take(30) // debug
            .ToList();

    public async Task RunMigration(List<AnkiNote> notes)
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
        var lines = note.Remarks.Split(["<br>", "<br />"], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (lines.Length != 2)
        {
            throw new InvalidOperationException($"Unexpected number of lines ({lines.Length}) in {note.Remarks}.");

        }
        var remarksLine = lines[0].Trim();
        var infinitiveLine = lines[1].Trim();

        var remarksLineSplit = remarksLine.Split("=", StringSplitOptions.TrimEntries);
        var conjugatedFormPl = remarksLineSplit[0].Trim();
        var conjugatedFormEs = remarksLineSplit[1].Trim();

        var sentenceEs = note.FrontText;
        var sentencePl = note.BackText;

        note.FrontText = sentencePl;
        note.FrontAudio = "";
        note.BackText = sentenceEs;
        note.BackAudio = "";
        note.Remarks = $"{conjugatedFormPl} = {conjugatedFormEs}<br />\n" +
                       $"{infinitiveLine}";

    }
}
