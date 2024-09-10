using HtmlAgilityPack;

namespace AnkiCardValidator.Utilities;

/// <summary>
/// Allows to work with the "Remarks" field of my Anki notes.
/// I use "Remarks" to keep unstructured data related to the flashcard that doesn't fit into other fields. For example:
/// - comments about the etymology
/// - my own notes added during the review
/// - AI-generated comments added by tooling, e.g. warnings that there might be some mistake in the flashcard that requires attention
/// - translation of the flashcard to another language (than the one on the back side of th flashcard), to clarify the meaning
/// </summary>
public static class RemarksUpdater
{
    /// <summary>
    /// Adds or updates a remark in the "Remarks" field of the Anki note.
    /// </summary>
    /// <param name="existingRemarks">The existing remarks string.</param>
    /// <param name="class">
    /// Arbitrary ID of the type of the remark, e.g. the name of the tool that generated the remark.
    /// Allows to later recognize that the remark was added by a specific tool and update or remove it without affecting
    /// other remarks present in the string.
    /// </param>
    /// <param name="remark">Content of the remark. Might contain HTML tags.</param>
    /// <returns>The updated remarks string.</returns>
    public static string AddOrUpdateRemark(this string? existingRemarks, string @class, string remark)
    {
        existingRemarks = existingRemarks ?? string.Empty;

        // use HtmlAgilityPack to parse the HTML and update the content
        var doc = new HtmlDocument();
        doc.LoadHtml(existingRemarks);

        var isValidHtml = IsValidHtml(doc, existingRemarks);
        if (!isValidHtml) return existingRemarks;

        // find if there is an existing div with the class `@class`
        var existingDiv = doc.DocumentNode.SelectSingleNode($"//div[contains(@class, '{@class}')]");
        if (existingDiv != null)
        {
            // update the content of the existing div
            existingDiv.InnerHtml = remark;
        }
        else
        {
            // create a new div with the class `@class` and the content `remark`
            var newDiv = HtmlNode.CreateNode($"<div class=\"{@class}\">{remark}</div>");
            doc.DocumentNode.AppendChild(newDiv);
        }

        return doc.DocumentNode.OuterHtml;
    }

    private static bool IsValidHtml(HtmlDocument doc, string originalHtml)
    {
        if (doc.ParseErrors != null && doc.ParseErrors.Any())
        {
            var parseErrorsSerialized = String.Join(", ", doc.ParseErrors.Select(x => x.Reason));
            Console.WriteLine($"The existing remarks string is not a valid HTML. {parseErrorsSerialized} in {originalHtml}");
            return false;
        }
        return true;
    }

    /// <summary>
    /// Removes a remark from the "Remarks" field of the Anki note.
    /// </summary>
    /// <param name="existingRemarks">The existing remarks string.</param>
    /// <param name="class">The ID of the remark to remove.</param>
    /// <returns>The updated remarks string.</returns>
    public static string RemoveRemark(this string? existingRemarks, string @class)
    {
        existingRemarks = existingRemarks ?? string.Empty;

        // Use HtmlAgility pack to remove div with the class `@class` (only if it exists)
        var doc = new HtmlDocument();
        doc.LoadHtml(existingRemarks);

        var isValidHtml = IsValidHtml(doc, existingRemarks);
        if (!isValidHtml) return existingRemarks;

        // find if there is an existing div with the class `@class`
        var existingDiv = doc.DocumentNode.SelectSingleNode($"//div[contains(@class, '{@class}')]");
        if (existingDiv != null)
        {
            existingDiv.Remove();
        }

        return doc.DocumentNode.OuterHtml;
    }

    public static bool HasRemark(this string? existingRemarks, string @class)
    {
        var remark = TryGetRemark(existingRemarks, @class);
        return remark != null;
    }

    public static string? TryGetRemark(this string? existingRemarks, string @class)
    {
        existingRemarks = existingRemarks ?? string.Empty;

        // Use HtmlAgility pack to determine if div with the class `@class` exists
        var doc = new HtmlDocument();
        doc.LoadHtml(existingRemarks);

        var isValidHtml = IsValidHtml(doc, existingRemarks);
        if (!isValidHtml) return null;

        // find if there is an existing div with the class `@class`
        var existingDiv = doc.DocumentNode.SelectSingleNode($"//div[contains(@class, '{@class}')]");
        if (existingDiv is null) return null;

        return existingDiv.OuterHtml;
    }

}
