using AnkiCardValidator.ViewModels;
using Spectre.Console;

namespace UpdateField.Utilities;
internal static class UiHelper
{
    public static void DisplayAnkiNote(AnkiNote firstNote)
    {
        var table = new Table();
        table.AddColumn("Field");
        table.AddColumn("Value");
        table.AddRow("FrontText", Markup.Escape(firstNote.FrontText));
        table.AddRow("FrontAudio", Markup.Escape(firstNote.FrontAudio));
        table.AddRow("BackText", $"[green]{(Markup.Escape(firstNote.BackText))}[/]"); // Set the style of the row to green
        table.AddRow("BackAudio", Markup.Escape(firstNote.BackAudio));
        table.AddRow("Examples", Markup.Escape(firstNote.Comments));
        AnsiConsole.Write(table);
    }
}
