using AnkiCardValidator;
using AnkiCardValidator.Utilities;
using AnkiCardValidator.ViewModels;
using HtmlAgilityPack;
using UpdateField.Utilities;

namespace UpdateField.Mutations;
public static class MoveImageToImageField
{
    public static List<AnkiNote> LoadNotesThatRequireAdjustment() =>
        AnkiHelpers.GetNotes(Settings.AnkiDatabaseFilePath, deckName: "2a. Ukrainian podcast S03")
            //.Take(30) // debug
            .ToList();

    public static void RunMigration(List<AnkiNote> notes)
    {
        foreach (var note in notes)
        {
            MigrateImageToImageField(note);
        }
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

        // If frontTextHtml contains only image tag, don't modify the card
        if (frontTextHtml.DocumentNode.ChildNodes.Count == 1 && frontTextHtml.DocumentNode.FirstChild.Name == "img") return;
        // If backTextHtml contains only image tag, don't modify the card
        if (backTextHtml.DocumentNode.ChildNodes.Count == 1 && backTextHtml.DocumentNode.FirstChild.Name == "img") return;

        if (frontTextImage != null)
        {
            note.Image = frontTextImage.OuterHtml;
            frontTextImage.Remove();

            MutationHelpers.TrimLeadingAndTrailingLineBreaks(frontTextHtml);
            note.FrontText = frontTextHtml.DocumentNode.InnerHtml.Trim();
        }

        if (backTextImage != null)
        {
            note.Image = backTextImage.OuterHtml;
            backTextImage.Remove();
            MutationHelpers.TrimLeadingAndTrailingLineBreaks(backTextHtml);
            note.BackText = backTextHtml.DocumentNode.InnerHtml.Trim();
        }
    }
}
