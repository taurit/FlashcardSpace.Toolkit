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
    /// <param name="class">
    /// Arbitrary ID of the type of the remark, e.g. the name of the tool that generated the remark.
    /// Allows to later recognize that the remark was added by a specific tool and update or remove it without affecting
    /// other remarks present in the string.
    /// </param>
    /// <param name="remark">Content of the remark. Might contain HTML tags.</param>
    /// <returns></returns>
    public static string AddOrUpdateRemark(this string? existingRemarks, string @class, string remark)
    {
        return existingRemarks;
    }

    public static string RemoveRemark(this string? existingRemarks, string @class)
    {
        return existingRemarks;
    }
}
