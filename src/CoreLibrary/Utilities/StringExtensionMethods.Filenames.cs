using System.Globalization;
using System.Text;

namespace CoreLibrary.Utilities;

public static class StringExtensionMethodsFilenames
{
    /// <summary>
    /// Transforms input string into a filename-friendly string.
    /// The filename should:
    /// - be fine to use on Windows, Linux, and macOS
    /// - retain the readability of the original string (keep the characters that are acceptable, replace ones that aren't)
    /// - be shortened to a reasonable length if the input is too long
    /// </summary>
    /// <param name="input">The string to transform</param>
    /// <param name="maxFilenameLength">255 is safe as file name on all OSes, but this will usually only be a fragment of a file name</param>
    public static string ToFilenameFriendlyString(this string input, int maxFilenameLength = 30)
    {
        // replace characters that are not allowed in filenames + spaces
        var invalidChars = new HashSet<char>(Path.GetInvalidFileNameChars()) {
            ' ',
            // needed to make extraction of filename from `[sound:filename.mp3]` Anki tag easier
            ']',
            '['
        };

        var filenameFriendly = new StringBuilder();
        foreach (var c in input)
        {
            if (invalidChars.Contains(c))
                filenameFriendly.Append('_');
            else
                filenameFriendly.Append(c);
        }

        var filename = filenameFriendly.ToString();
        // shorten the filename if it's too long
        if (filename.Length > maxFilenameLength)
        {
            filename = filename.Substring(0, maxFilenameLength);
        }

        // replace characters with national accents with their ASCII equivalents (ą -> a, etc.) 
        filename = RemoveDiacritics(filename);

        return filename;
    }

    private static string RemoveDiacritics(string input)
    {
        if (input == null)
            return null;

        // Normalize the string to FormD, which separates diacritics from characters
        string normalizedString = input.Normalize(NormalizationForm.FormD);

        var stringBuilder = new StringBuilder();

        // Iterate through the normalized string and only keep non-diacritical characters
        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        // Normalize back to the original form
        return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
    }
}

