using AnkiCardValidator;
using AnkiCardValidator.Utilities;
using AnkiCardValidator.ViewModels;
using HtmlAgilityPack;
using Spectre.Console;

namespace UpdateField.Mutations;
public static class MoveImageToImageField
{
    public static List<AnkiNote> LoadNotesThatRequireAdjustment() =>
        AnkiHelpers.GetNotes(Settings.AnkiDatabaseFilePath, deckName: "2. Ukrainian")
            .Take(30) // debug
            .ToList();

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
            frontTextImage.Remove();

            TrimLeadingAndTrailingLineBreaks(frontTextHtml);
            note.FrontText = frontTextHtml.DocumentNode.InnerHtml.Trim();
        }

        if (backTextImage != null)
        {
            note.Image = backTextImage.OuterHtml;
            backTextImage.Remove();
            TrimLeadingAndTrailingLineBreaks(backTextHtml);
            note.BackText = backTextHtml.DocumentNode.InnerHtml.Trim();
        }
    }

    /// <summary>
    /// If given HTML starts or ends with HTML break tag (e.g. <br>, <br />, <BR/>, <BR />) remove it using HtmlAgilityPack.
    /// </summary>
    private static void TrimLeadingAndTrailingLineBreaks(HtmlDocument frontTextHtml)
    {
        // remove leading newlines/whitespace
        do
        {
            var firstNode = frontTextHtml.DocumentNode.ChildNodes.First();
            var firstNodeIsLineBreak = firstNode.Name == "br";
            var firstNodeIsWhitespace = firstNode.Name == "#text" && String.IsNullOrWhiteSpace(firstNode.InnerText);
            var firstNodeIsEmptyDiv = firstNode.Name == "div" && !firstNode.HasChildNodes && String.IsNullOrWhiteSpace(firstNode.InnerText);
            var firstNodeIsDivWithOnlyLineBreakOrWhitespace = firstNode.Name == "div" &&
                                                              firstNode.ChildNodes.All(cn =>
                                                                cn.Name == "br" ||
                                                                (cn.Name == "#text" && String.IsNullOrWhiteSpace(cn.InnerText))
                                                              );

            if (!firstNodeIsLineBreak && !firstNodeIsWhitespace && !firstNodeIsEmptyDiv && !firstNodeIsDivWithOnlyLineBreakOrWhitespace) break;
            frontTextHtml.DocumentNode.RemoveChild(firstNode);
        } while (true);

        // remove trailing newlines/whitespace
        do
        {
            var lastNode = frontTextHtml.DocumentNode.ChildNodes.Last();
            var lastNodeIsLineBreak = lastNode.Name == "br";
            var lastNodeIsWhitespace = lastNode.Name == "#text" && String.IsNullOrWhiteSpace(lastNode.InnerText);
            var lastNodeIsEmptyDiv = lastNode.Name == "div" && !lastNode.HasChildNodes && String.IsNullOrWhiteSpace(lastNode.InnerText);
            var lastNodeIsDivWithOnlyLineBreakOrWhitespace = lastNode.Name == "div" &&
                                                             lastNode.ChildNodes.All(cn =>
                                                                  cn.Name == "br" ||
                                                                  (cn.Name == "#text" && String.IsNullOrWhiteSpace(cn.InnerText))
                                                              );

            if (!lastNodeIsLineBreak && !lastNodeIsWhitespace && !lastNodeIsEmptyDiv && !lastNodeIsDivWithOnlyLineBreakOrWhitespace) break;
            frontTextHtml.DocumentNode.RemoveChild(lastNode);
        } while (true);
    }
}
