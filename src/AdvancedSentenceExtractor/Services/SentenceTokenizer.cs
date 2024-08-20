using AdvancedSentenceExtractor.Models;
using System.Text;
using System.Text.RegularExpressions;

namespace AdvancedSentenceExtractor.Services;

public class SentenceTokenizer
{
    private static Regex _numberPattern = new Regex("^\\d+\\.?$", RegexOptions.Compiled);
    private static Regex _uselessSentencePattern = new Regex("^[\\d\\.—]+\\.$", RegexOptions.Compiled);
    private readonly SentenceFactory _sentenceFactory;

    public SentenceTokenizer(SentenceFactory sentenceBuilder)
    {
        _sentenceFactory = sentenceBuilder;
    }

    public List<Sentence> TokenizeBook(string bookContent)
    {
        bookContent = KnownAbbreviationsHandler.ReplaceDotWithFullWidthDotInAbbreviations(bookContent);
        bookContent = EllipsisHandler.ReplaceEllipsisWithSingleCharacter(bookContent);
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
                var currentSentenceCandidate = currentSentence.ToString().Trim().Trim(new char[] { '‘', '’' });
                if (currentSentenceCandidate.Length > 1 &&
                    !string.IsNullOrWhiteSpace(currentSentenceCandidate) &&
                    !_uselessSentencePattern.IsMatch(currentSentenceCandidate)
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
                    var sentence = _sentenceFactory.BuildSentence(sanitizedSentence, previousSentence);
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

        var lines = bookContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

        StringBuilder bookContentWithPunctuationMarksAdded = new StringBuilder();
        foreach (var line in lines)
        {
            // assuming calibre artifact - empty line with just a page number
            if (_numberPattern.IsMatch(line))
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
