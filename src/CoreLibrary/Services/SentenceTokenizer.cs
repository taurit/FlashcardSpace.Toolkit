using CoreLibrary.Interfaces;
using CoreLibrary.Utilities;
using System.Text;
using System.Text.RegularExpressions;

namespace CoreLibrary.Services;

public class SentenceTokenizer(SentenceFactory sentenceBuilder)
{
    private static readonly Regex NumberPattern = new("^\\d+\\.?$", RegexOptions.Compiled);
    private static readonly Regex UselessSentencePattern = new("^[\\d\\.—]+\\.$", RegexOptions.Compiled);

    public List<Sentence> TokenizeBook(string bookContent)
    {
        bookContent = KnownAbbreviationsHandler.ReplaceDotWithFullWidthDotInAbbreviations(bookContent);
        bookContent = bookContent.ReplaceEllipsisWithSingleCharacter();
        bookContent = AddMissingPunctuationMarksBeforeNewLines(bookContent);

        Sentence? previousSentence = null;

        var sentences = new List<Sentence>();

        var currentSentence = new StringBuilder();
        for (var i = 0; i < bookContent.Length; i++)
        {
            var currentChar = bookContent[i];

            currentSentence.Append(currentChar);

            if (IsSentenceEndingCharacter(currentChar, bookContent, i))
            {
                var currentSentenceCandidate = currentSentence.ToString().Trim().Trim('‘', '’');
                if (currentSentenceCandidate.Length > 1 &&
                    !string.IsNullOrWhiteSpace(currentSentenceCandidate) &&
                    !UselessSentencePattern.IsMatch(currentSentenceCandidate)
                    )
                {
                    var sanitizedSentence = currentSentenceCandidate
                        .Replace("\r\n", " ")
                        .Replace("\r\n", " ")
                        .Replace("\r\n", " ")
                        .Replace("\r\n", " ")
                        .Replace("\r\n", " ")
                        .Replace("  ", " ")
                        .Replace("  ", " ")
                        .Replace("  ", " ")
                        .Replace("  ", " ")
                        .Replace("  ", " ")
                        ;

                    sanitizedSentence = KnownAbbreviationsHandler.ReplaceFullWidthDotWithDotInAbbreviations(sanitizedSentence);
                    var sentence = sentenceBuilder.BuildSentence(sanitizedSentence, previousSentence);
                    if (sentence.HasAnyWords)
                    {
                        sentences.Add(sentence);
                        previousSentence = sentence;
                    }
                }

                currentSentence.Clear();
            }
        }

        return sentences;
    }


    /// <summary>
    /// Pre-processes text, so sentences end with one of the approved single-character punctuation marks:
    /// - '.'
    /// - '?'
    /// - '!'
    /// - '…'
    /// - ';'
    /// - ':'
    ///
    /// ... where in the original text it ends with a new line only. This often happens in dialogues.
    /// </summary>
    private string AddMissingPunctuationMarksBeforeNewLines(string bookContent)
    {
        // cant remember what it was for, lets see when it fils
        //bookContent = Regex.Replace(bookContent, @" (\p{Lu})\.", " $1．"); // note a fullwidthdot

        var lines = bookContent.Split(["\r\n", "\r", "\n"], StringSplitOptions.None);

        StringBuilder bookContentWithPunctuationMarksAdded = new StringBuilder();
        foreach (var line in lines)
        {
            // assuming calibre artifact - empty line with just a page number
            if (NumberPattern.IsMatch(line))
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(line)) continue; // don't output empty lines


            if (line.EndsWith(".") | line.EndsWith("?") | line.EndsWith("!") | line.EndsWith("…"))
            {
                // an approved punctuation mark is already present
                bookContentWithPunctuationMarksAdded.AppendLine($"{line}");
            }
            else
            {
                // add a dot as a default punctuation mark
                bookContentWithPunctuationMarksAdded.AppendLine($"{line}.");
            }
        }
        bookContent = bookContentWithPunctuationMarksAdded.ToString();
        return bookContent;
    }


    private static bool IsSentenceEndingCharacter(char currentChar, string bookContent, int currentIndex)
    {
        var isEndOfText = currentIndex == bookContent.Length - 1;
        var isPunctuationCharacter =
            currentChar == '.' ||
            currentChar == '!' ||
            currentChar == '?' ||
            currentChar == ';' ||
            currentChar == '…' ||
            currentChar == ':' ||
            currentChar == '»' // ukrainian translation uses that for chapter titles 
            ;

        return isPunctuationCharacter || isEndOfText;
    }
}
