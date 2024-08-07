using AnkiCardValidator;
using AnkiCardValidator.Utilities;
using AnkiCardValidator.ViewModels;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace UpdateField.Mutations;
public static class CleanLongmanSet
{
    public static List<AnkiNote> LoadNotesThatRequireAdjustment() =>
        AnkiHelpers.GetNotes(Settings.AnkiDatabaseFilePath, limitToTag: "longman-picture-dictionary")
            //.Take(30) // debug
            .ToList();

    public static void RunMigration(List<AnkiNote> notes)
    {
        foreach (var note in notes)
        {
            CleanUpBackAudioField(note);
            CleanUpRemarksField(note);
            CleanUpRemarksFieldFromLinks(note);
        }
    }

    private static void CleanUpBackAudioField(AnkiNote note)
    {
        // Use regex to clean up the `note.BackAudio` string and only keep the Anki audio tags (example: `[sound:wastebasket_us.mp3]`)
        // If the field is empty, don't modify it
        // If the field is already clean, don't modify it

        // Example of a dirty field:
        // <span class="speaker amefile fa fa-volume-up">[sound:wastebasket_us.mp3]</span></span>

        // Example of a cleaned field:
        // [sound:wastebasket_us.mp3]

        var backAudio = note.BackAudio;
        if (String.IsNullOrWhiteSpace(backAudio)) return;

        // find all [sound:...] tags
        var matches = Regex.Matches(backAudio, @"\[sound:[^\]]+\]");
        if (matches.Count == 0) return;

        // build new string with only [sound:...] tags
        var newBackAudio = String.Join("", matches);
        note.BackAudio = newBackAudio;
    }

    /// <summary>
    /// Modified the note.Remarks field to remove any unnecessary HTML tags and keep only the following ones:
    /// - <span class="DEF">...</span>
    /// - <span class="BREQUIV">...</span>
    /// - <span class="AMEQUIV">...</span>
    /// </summary>
    private static void CleanUpRemarksField(AnkiNote note)
    {
        // Use HtmlAgilityPack to parse the `note.Remarks` field and only keep the necessary tags

        if (String.IsNullOrWhiteSpace(note.Remarks)) return;

        var html = new HtmlDocument();
        html.LoadHtml(note.Remarks);

        var newHtml = new HtmlDocument();

        // find div with class `DEF` in original HTML
        var defNode = html.DocumentNode.SelectSingleNode("//span[@class='DEF']");
        if (defNode != null)
        {
            newHtml.DocumentNode.AppendChild(defNode);
        }

        // find div with class `BREQUIV` in original HTML
        var brequivNode = html.DocumentNode.SelectSingleNode("//span[@class='BREQUIV']");
        if (brequivNode != null)
        {
            newHtml.DocumentNode.AppendChild(brequivNode);
        }

        // find div with class `AMEQUIV` in original HTML
        var amequivNode = html.DocumentNode.SelectSingleNode("//span[@class='AMEQUIV']");
        if (amequivNode != null)
        {
            newHtml.DocumentNode.AppendChild(amequivNode);
        }

        if (newHtml.DocumentNode.ChildNodes.Count != 0)
        {
            note.Remarks = newHtml.DocumentNode.OuterHtml;
        }
    }

    /// <summary>
    /// Remove HTML <a></a> tags from the content of the `note.Remarks` field (but keep the InnerText)
    /// </summary>
    private static void CleanUpRemarksFieldFromLinks(AnkiNote note)
    {
        var html = new HtmlDocument();
        html.LoadHtml(note.Remarks);

        // find all <a> tags
        var aNodes = html.DocumentNode.SelectNodes("//a");
        if (aNodes == null) return;
        foreach (var aNode in aNodes)
        {
            var innerText = aNode.InnerText;
            aNode.ParentNode.ReplaceChild(HtmlNode.CreateNode(innerText), aNode);
        }

        note.Remarks = html.DocumentNode.OuterHtml;
    }

}
