using System.Text.RegularExpressions;

namespace CoreLibrary.Utilities;

public static partial class SubtitlesHelperSrt
{
    [GeneratedRegex(@"^\s*\d+\s*$|^\s*\d{2}:\d{2}:\d{2},\d{3}.*$", RegexOptions.Multiline | RegexOptions.Compiled)]
    private static partial Regex UnwantedSubstrings();

    /// <summary>
    /// Removes all timestamps and sequence numbers from the subtitles file content.
    /// Leaves only subtitle text (also without html markup).
    /// </summary>
    /// <remarks>
    /// *.srt format contains blocks like:
    /// <code>
    /// 1
    /// 00:00:00,835 --&gt; 00:00:03,254
    /// Viserys Targaryen:
    /// I, Viserys Targaryen,
    ///
    /// 2
    /// 00:00:03,796 --&gt; 00:00:05,757
    /// Lord of the Seven Kingdoms,
    /// </code>
    ///
    /// There is a room for inconsistency, and Video Players generally rely on the timestamps, not the sequence number or order of blocks.
    /// In theory, I should then order the blocks by timestamps, but let's try to not mess with the order and just assume that
    /// the blocks are already ordered by timestamps (why wouldn't they be?).
    /// 
    /// </remarks>
    public static string ConvertSubtitlesToText(string subtitlesFileContent)
    {
        var cleanedContent = UnwantedSubstrings().Replace(subtitlesFileContent, string.Empty);

        // reduce whitespace character strings to single space
        cleanedContent = Regex.Replace(cleanedContent, @"\s{2,}", " ");

        return cleanedContent.Trim();
    }
}
