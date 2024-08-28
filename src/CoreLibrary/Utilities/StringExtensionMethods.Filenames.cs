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
    public static string GetFilenameFriendlyString(this string input)
    {
        const int maxFilenameLength = 30; // 255 is safe as file name on all OSes, but this will usually only be a fragment of a file name

        // replace characters that are not allowed in filenames
        var invalidChars = new HashSet<char>(Path.GetInvalidFileNameChars());
        var filenameFriendly = new StringBuilder();
        foreach (var c in input)
        {
            if (invalidChars.Contains(c))
                filenameFriendly.Append('_');
            else
                filenameFriendly.Append(c);
        }

        // shorten the filename if it's too long
        if (filenameFriendly.Length > maxFilenameLength)
        {
            var filenameFriendlyShortened = filenameFriendly.ToString().Substring(0, maxFilenameLength);
            return filenameFriendlyShortened;
        }

        return filenameFriendly.ToString();
    }
}

