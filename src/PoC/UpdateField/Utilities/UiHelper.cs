using AnkiCardValidator.Utilities;
using AnkiCardValidator.ViewModels;
using Spectre.Console;

namespace UpdateField.Utilities;
internal static class UiHelper
{

    public static void DisplayModifiedNotesDiff(List<AnkiNote> modifiedNotes)
    {
        AnsiConsole.MarkupLine($"[aqua]The following {modifiedNotes.Count} notes have modifications:[/]");

        foreach (var modifiedNote in modifiedNotes)
        {
            DisplayAnkiNote(modifiedNote);
        }
    }

    private static void DisplayAnkiNote(AnkiNote note)
    {
        // parse fields to see exactly which ones were modified
        var originalNote = new AnkiNote(0, "OneDirection", "", note.FieldsRawOriginal);

        var table = new Table();
        table.AddColumn("Field");
        table.AddColumn("Value");

        DisplayDiff(table, nameof(note.FrontText), note.FrontText, originalNote.FrontText);
        DisplayDiff(table, nameof(note.FrontAudio), note.FrontAudio, originalNote.FrontAudio);
        DisplayDiff(table, nameof(note.BackText), note.BackText, originalNote.BackText);
        DisplayDiff(table, nameof(note.BackAudio), note.BackAudio, originalNote.BackAudio);
        DisplayDiff(table, nameof(note.Image), note.Image, originalNote.Image);
        DisplayDiff(table, nameof(note.Remarks), note.Remarks, originalNote.Remarks);

        AnsiConsole.Write(table);
    }

    /// <summary>
    /// Display in diff style: red for removed, green for added
    /// </summary>
    private static void DisplayDiff(Table table, string fieldName, string current, string previous)
    {
        if (current != previous)
        {
            table.AddRow($"[red]{fieldName}[/]", $"[red]{(Markup.Escape(previous))}[/]");
            table.AddRow($"[green]{fieldName}[/]", $"[green]{(Markup.Escape(current))}[/]");
        }
        else
        {
            // some fields are so boring they can be skipped in diff view unless they changed
            if (fieldName == nameof(AnkiNote.FrontAudio)) return;
            if (fieldName == nameof(AnkiNote.BackAudio)) return;
            if (fieldName == nameof(AnkiNote.Image)) return;

            var stringWithoutAccentMarksUnsupportedByCmd = current.RemoveUkrainianFlashcardsAccentMark();
            table.AddRow(fieldName, Markup.Escape(stringWithoutAccentMarksUnsupportedByCmd));
        }
    }

}
