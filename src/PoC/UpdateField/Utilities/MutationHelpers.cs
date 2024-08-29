using AnkiCardValidator.ViewModels;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace UpdateField.Utilities;

/// <summary>
/// Potentially re-usable functions that modify AnkiNote fields
/// </summary>
public static class MutationHelpers
{
    /// <summary>
    /// Remove HTML <a></a> tags from the content of the `note.Remarks` field (but keep the InnerText)
    /// </summary>
    private static void RemoveLinksFromRemarksField(AnkiNote note)
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

    public static string TrimLeadingAndTrailingLineBreaks(string htmlString)
    {
        var html = new HtmlDocument();
        html.LoadHtml(htmlString);
        TrimLeadingAndTrailingLineBreaks(html);
        return html.DocumentNode.OuterHtml;
    }

    /// <summary>
    /// If given HTML starts or ends with HTML break tag (e.g. <br>, <br />, <BR/>, <BR />) remove it using HtmlAgilityPack.
    /// </summary>
    public static void TrimLeadingAndTrailingLineBreaks(HtmlDocument html)
    {
        // remove leading newlines/whitespace
        do
        {
            var firstNode = html.DocumentNode.ChildNodes.First();
            var firstNodeIsLineBreak = firstNode.Name == "br";
            var firstNodeIsWhitespace = firstNode.Name == "#text" && String.IsNullOrWhiteSpace(firstNode.InnerText);
            var firstNodeIsEmptyDiv = firstNode.Name == "div" && !firstNode.HasChildNodes && String.IsNullOrWhiteSpace(firstNode.InnerText);
            var firstNodeIsDivWithOnlyLineBreakOrWhitespace = firstNode.Name == "div" &&
                                                              firstNode.ChildNodes.All(cn =>
                                                                  cn.Name == "br" ||
                                                                  (cn.Name == "#text" && String.IsNullOrWhiteSpace(cn.InnerText))
                                                              );

            if (!firstNodeIsLineBreak && !firstNodeIsWhitespace && !firstNodeIsEmptyDiv && !firstNodeIsDivWithOnlyLineBreakOrWhitespace) break;
            html.DocumentNode.RemoveChild(firstNode);
        } while (true);

        // remove trailing newlines/whitespace
        do
        {
            var lastNode = html.DocumentNode.ChildNodes.Last();
            var lastNodeIsLineBreak = lastNode.Name == "br";
            var lastNodeIsWhitespace = lastNode.Name == "#text" && String.IsNullOrWhiteSpace(lastNode.InnerText);
            var lastNodeIsEmptyDiv = lastNode.Name == "div" && !lastNode.HasChildNodes && String.IsNullOrWhiteSpace(lastNode.InnerText);
            var lastNodeIsDivWithOnlyLineBreakOrWhitespace = lastNode.Name == "div" &&
                                                             lastNode.ChildNodes.All(cn =>
                                                                 cn.Name == "br" ||
                                                                 (cn.Name == "#text" && String.IsNullOrWhiteSpace(cn.InnerText))
                                                             );

            if (!lastNodeIsLineBreak && !lastNodeIsWhitespace && !lastNodeIsEmptyDiv && !lastNodeIsDivWithOnlyLineBreakOrWhitespace) break;
            html.DocumentNode.RemoveChild(lastNode);
        } while (true);
    }

    public static void MoveSoundTagFromFrontTextToFrontAudio(AnkiNote note)
    {
        var frontText = note.FrontText;
        if (String.IsNullOrWhiteSpace(frontText)) return;

        // find all [sound:...] tags
        var matches = Regex.Matches(frontText, @"\[sound:[^\]]+\]");
        if (matches.Count == 0) return;

        // build new string with only [sound:...] tags
        var newFrontAudio = String.Join("", matches);
        note.FrontAudio = newFrontAudio;

        // remove audio from FrontText
        foreach (Match match in matches)
        {
            note.FrontText = note.FrontText.Replace(match.Value, "");
        }

        // trim leading and trailing line breaks that could have been left after removing audio
        var frontTextHtml = new HtmlDocument();
        frontTextHtml.LoadHtml(note.FrontText);
        TrimLeadingAndTrailingLineBreaks(frontTextHtml);
        note.FrontText = frontTextHtml.DocumentNode.OuterHtml.Trim();
    }

    /// <summary>
    /// If a given note has an image in the FrontText field or BackText field, move it to the Image field.
    /// </summary>
    public static void MigrateImageFromBackTextToImage(AnkiNote note)
    {
        if (!String.IsNullOrWhiteSpace(note.Image)) return;

        // Use HTMLAgilityPack to detect if there's an image in the BackText field
        var backText = note.BackText;

        var backTextHtml = new HtmlDocument();
        backTextHtml.LoadHtml(backText);
        var backTextImage = backTextHtml.DocumentNode.SelectSingleNode("//img");

        // If there are images in both question and answer, the card was likely modified manually by me and
        // does not follow a typical pattern
        if (backTextImage != null) return;

        // If backTextHtml contains only image tag, don't modify the card
        if (backTextHtml.DocumentNode.ChildNodes.Count == 1 && backTextHtml.DocumentNode.FirstChild.Name == "img") return;

        if (backTextImage != null)
        {
            note.Image = backTextImage.OuterHtml;
            backTextImage.Remove();
            TrimLeadingAndTrailingLineBreaks(backTextHtml);
            note.BackText = backTextHtml.DocumentNode.InnerHtml.Trim();
        }
    }

    public static void RemoveWrapperDivs(AnkiNote note)
    {
        var frontText = note.FrontText;
        var backText = note.BackText;
        var remarks = note.Remarks;
        var image = note.Image;

        note.FrontText = RemoveAllWrapperDivs(frontText);
        note.BackText = RemoveAllWrapperDivs(backText);
        note.Remarks = RemoveAllWrapperDivs(remarks);
        note.Image = RemoveAllWrapperDivs(image);

    }

    /// <summary>
    /// Performs removal until nothing more can be removed
    /// </summary>
    private static string RemoveAllWrapperDivs(string htmlContent)
    {
        while (true)
        {
            var newHtmlContent = RemoveSingleWrapperDiv(htmlContent);
            if (newHtmlContent == htmlContent) return htmlContent;

            htmlContent = newHtmlContent;
        }
    }

    /// <summary>
    /// Removes wrapping DIVs from the given HTML content but only if:
    /// - they are the only top-level tag (they don't have siblings)
    /// - they have no attributes.
    ///
    /// Examples of divs that should be removed:
    /// - <div>Front text</div>
    /// - <DIV>Back text</DIV>
    /// - <DiV>Image</div>
    /// - <div><div>Image</div></div>
    /// 
    /// Examples of divs that should NOT be removed:
    /// - <div id="test">Front text</div>
    /// - <div class='test'>Back text</div>
    /// - <div style=''>Image</div>
    /// - <div isDisabled>Remarks</div>
    /// - <div></div><div>Front text</div>
    /// - <b></b>Back <div>text</div>
    /// - <div>Text 1</div><span>AAA</span>
    ///  
    /// </summary>
    internal static string RemoveSingleWrapperDiv(string htmlContent)
    {
        if (String.IsNullOrWhiteSpace(htmlContent)) return htmlContent;

        var html = new HtmlDocument();
        html.LoadHtml(htmlContent);

        // if there's only one child node and it's a div with no attributes, remove the div
        if (html.DocumentNode.ChildNodes.Count == 1 && html.DocumentNode.FirstChild.Name == "div" && !html.DocumentNode.FirstChild.HasAttributes)
        {
            var divContent = html.DocumentNode.FirstChild.InnerHtml;
            var divContentTrimmed = TrimLeadingAndTrailingLineBreaks(divContent);
            return divContentTrimmed;
        }

        return TrimLeadingAndTrailingLineBreaks(htmlContent);

    }
}
