using System.Text.RegularExpressions;

namespace CoreLibrary.Services;

public interface IWordTokenizer
{
    List<string> GetWords(string sentence);
}

public partial class WordTokenizer : IWordTokenizer
{
    [GeneratedRegex(@"[0-9\p{L}'’．]+")]
    private static partial Regex _removeIrrelevantCharactersPattern();

    [GeneratedRegex(@"^['’．]*$")]
    private static partial Regex _irrelevantWordCandidatePattern();

    private static readonly char[] TrimCharacters = ['\n', '\r', '.', '’', ' ', '\''];
    public List<string> GetWords(string sentence)
    {
        if (string.IsNullOrEmpty(sentence))
            return [];

        sentence = KnownAbbreviationsHandler.ReplaceDotWithFullWidthDotInAbbreviations(sentence);

        // regex pattern to match word characters and apostrophes within words
        var matches = _removeIrrelevantCharactersPattern().Matches(sentence);

        var words = new List<string>(matches.Count);
        foreach (Match match in matches)
        {
            var word = match.Value.Trim(TrimCharacters);
            word = KnownAbbreviationsHandler.ReplaceFullWidthDotWithDotInAbbreviations(word);

            if (_irrelevantWordCandidatePattern().IsMatch(word)) continue;

            word = word.Replace('’', '\'');
            words.Add(word);
        }

        return words;
    }
}
