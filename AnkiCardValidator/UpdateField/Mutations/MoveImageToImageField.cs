using AnkiCardValidator;
using AnkiCardValidator.Utilities;
using AnkiCardValidator.ViewModels;
using HtmlAgilityPack;
using Spectre.Console;

namespace UpdateField.Mutations;
public static class MoveImageToImageField
{
    public static List<AnkiNote> LoadNotesThatRequireAdjustment() =>
        AnkiHelpers.GetNotes(Settings.AnkiDatabaseFilePath, deckName: "2. Ukrainian").ToList();

    public static void RunMigration(List<AnkiNote> notes)
    {
        AnsiConsole.Progress()
            .Start(ctx =>
            {
                var progress = ctx.AddTask("Migrating images to the Image field...");
                progress.MaxValue = notes.Count;

                foreach (var note in notes)
                {
                    MigrateImageToImageField(note);
                    progress.Increment(1);
                }
            });

    }

    /// <summary>
    /// If a given note has an image in the FrontText field or BackText field, move it to the Image field.
    /// </summary>
    public static void MigrateImageToImageField(AnkiNote note)
    {
        if (!String.IsNullOrWhiteSpace(note.Image))
        {
            return;
        }

        // Use HTMLAgilityPack to detect if there's an image in the FrontText or BackText field
        var frontText = note.FrontText;
        var backText = note.BackText;

        var frontTextHtml = new HtmlDocument();
        frontTextHtml.LoadHtml(frontText);
        var frontTextImage = frontTextHtml.DocumentNode.SelectSingleNode("//img");

        var backTextHtml = new HtmlDocument();
        backTextHtml.LoadHtml(backText);
        var backTextImage = backTextHtml.DocumentNode.SelectSingleNode("//img");

        // If there are images in both question and answer, the card was likely modified manually by me and
        // does not follow a typical pattern
        if (frontTextImage != null && backTextImage != null) return;

        if (frontTextImage != null)
        {
            note.Image = frontTextImage.OuterHtml;
            frontTextHtml.DocumentNode.RemoveChild(frontTextImage);
            note.FrontText = frontTextHtml.DocumentNode.InnerHtml;
            note.FrontText = note.FrontText.Trim();

            // if FrontText starts or ends with HTML break tag (e.g. <br>, <br />, <BR/>, <BR />) remove it using HtmlAgilityPack
            // todo implement manually, xpath not needed
            var frontTextBreakTag = frontTextHtml.DocumentNode.SelectSingleNode("//br[1]"); // Select the first <br> element
            if (frontTextBreakTag != null)
            {
                frontTextHtml.DocumentNode.RemoveChild(frontTextBreakTag);
                note.FrontText = frontTextHtml.DocumentNode.InnerHtml;
                note.FrontText = note.FrontText.Trim();
            }
        }

        if (backTextImage != null)
        {
            note.Image = backTextImage.OuterHtml;
            backTextHtml.DocumentNode.RemoveChild(backTextImage);
            note.BackText = backTextHtml.DocumentNode.InnerHtml;
            note.BackText = note.BackText.Trim();
        }
    }
}
