using AnkiCardValidator.ViewModels;
using Spectre.Console;

namespace UpdateField.Utilities;
internal static class UiHelper
{
    public static void DisplayAnkiNote(AnkiNote note)
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
        DisplayDiff(table, nameof(note.Comments), note.Comments, originalNote.Comments);

        AnsiConsole.Write(table);
    }

    /// <summary>
    /// Display in diff style: red for removed, green for added
    /// </summary>
    private static void DisplayDiff(Table table, string fieldName, string current, string previous)
    {
        if (current != previous)
        {
            table.AddRow(fieldName, $"[red]{(Markup.Escape(previous))}[/]");
            table.AddRow(fieldName, $"[green]{(Markup.Escape(current))}[/]");
        }
        else
        {
            table.AddRow(fieldName, Markup.Escape(current));
        }
    }
}
