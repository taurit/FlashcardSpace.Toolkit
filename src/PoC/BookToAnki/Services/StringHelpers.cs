using System.Text.RegularExpressions;

namespace BookToAnki.Services;
public static class StringHelpers
{
    public static string ReplaceEnding(this string inputString, string replaceEndingFrom, string replaceEndingTo)
    {
        if (inputString.EndsWith(replaceEndingFrom))
        {
            int lastIndex = inputString.LastIndexOf(replaceEndingFrom);
            return inputString.Remove(lastIndex) + replaceEndingTo;
        }

        return inputString;
    }

    public static string ReplaceEndingRegex(this string inputString, string replaceEndingFromPattern, string replaceEndingTo)
    {
        // Compile the regular expression for efficiency
        Regex replaceEndingFromRegex = new Regex(replaceEndingFromPattern);

        // Try to find a match at the end of the string
        Match match = replaceEndingFromRegex.Match(inputString);
        if (match.Success && match.Index + match.Length == inputString.Length)
        {
            // If there's a match at the end, perform the replacement
            return inputString.Substring(0, match.Index) + replaceEndingFromRegex.Replace(match.Value, replaceEndingTo);
        }

        // If there's no match, return the original string
        return inputString;
    }

    public static string? GetJsonFromChatGptResponse(this string response)
    {
        // Pattern to find a code block optionally with a content type after triple backticks
        const string pattern = @"```(?:[a-zA-Z]+\s)?([\s\S]*?)(?:```)?\Z";

        // Use regular expression to find matches
        var match = Regex.Match(response, pattern);

        // Return the first group of the match if it's successful

        if (match.Success)
            return match.Groups[1].Value.Trim(new char[] { '`', '\n', '\r', '\t', ' ' }).Trim();
        else
        {
            throw new Exception($"Couldn't parse JSON fragment from the response: {response}");
        }
    }

}
