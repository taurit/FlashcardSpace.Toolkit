using System.Text.RegularExpressions;

namespace AnkiCardValidator.Utilities;

public class DefinitionCounter
{
    public int CountDefinitions(string word)
    {
        if (string.IsNullOrWhiteSpace(word))
        {
            return 0;
        }

        var sanitized = word;

        // remove everything after `<br />` if it's found
        var indexOfBr = sanitized.IndexOf("<br />", StringComparison.OrdinalIgnoreCase);
        if (indexOfBr != -1)
        {
            sanitized = sanitized.Substring(0, indexOfBr);
        }

        // remove everything after newline character
        var indexOfNewline = sanitized.IndexOf("\n", StringComparison.OrdinalIgnoreCase);
        if (indexOfNewline != -1)
        {
            sanitized = sanitized.Substring(0, indexOfNewline);
        }

        // remove everything in parentheses
        sanitized = Regex.Replace(sanitized, @"\([^)]*\)", "");

        // remove everything which looks like HTML tags (between `<` and `>`)
        sanitized = Regex.Replace(sanitized, @"<[^>]*>", "");

        // count the number of definitions
        var numDefinitions = sanitized.Split(',').Length;

        return numDefinitions;
    }
}
