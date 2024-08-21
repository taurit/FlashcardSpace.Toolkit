using System.Text.RegularExpressions;

namespace CoreLibrary.Utilities;
public static partial class StringExtensionMethodsOpenAI
{

    [GeneratedRegex(@"```(.*)```\Z", RegexOptions.Singleline)]
    private static partial Regex BackticksRegex();

    /// <summary>
    ///     Removes the triple backticks and the content type from the string (both at the beginning and at the end of a
    ///     string) if it exists.
    /// 
    ///     Input example:
    ///     ```html
    ///     <p>Content</p>
    ///     ```
    /// 
    ///     Output example:
    ///     <p>Content</p>
    /// </summary>
    public static string RemoveBackticksBlockWrapper(string input)
    {
        // Use regular expression to find matches
        var match = BackticksRegex().Match(input);

        // Return the first group of the match if it's successful

        if (match.Success)
        {
            return match.Groups[1].Value.Trim('`', '\n', '\r', '\t', ' ').Trim();
        }

        return input;
    }

}
