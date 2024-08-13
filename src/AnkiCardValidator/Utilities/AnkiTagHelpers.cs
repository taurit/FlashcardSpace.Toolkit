namespace AnkiCardValidator.Utilities;

public static class AnkiTagHelpers
{
    /// <summary>
    /// Parses tags from a string residing in `notes.tags`. Tags are separated by a space. Also, there is a leading space at the beginning, and a trailing space at the end of the string (most likely to simplify SQL queries, so they can use LIKE `%tag%` syntax).
    /// </summary>
    public static HashSet<string> ParseTags(string tagsString)
    {
        return tagsString.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries).ToHashSet();
    }

    /// <summary>
    /// Adds a tag to a string of tags. The tag is added only if it's not already there.
    /// Anki's tags are separated by a space. Also, there is a leading space at the beginning, and a trailing space at the end of the string (most likely to simplify SQL queries, so they can use LIKE `%tag%` syntax).
    /// Tags itself must not contain spaces and special characters, this method should throw exception if they do.
    /// </summary>
    /// <param name="tagToAdd">A tag to validate and add</param>
    /// <param name="tagsString">Existing tags</param>
    /// <returns></returns>
    public static string AddTagToAnkiTagsString(string tagToAdd, string tagsString)
    {
        if (tagToAdd.Contains(' ') || tagToAdd.Contains('%'))
        {
            throw new ArgumentException("Tag must not contain spaces or special characters.");
        }

        if (tagsString.Contains($" {tagToAdd} ")) return tagsString; // already has the tag

        var newTags = ParseTags(tagsString).Append(tagToAdd).Distinct();
        var newTagsString = $" {string.Join(' ', newTags)} ";
        return newTagsString;
    }
}