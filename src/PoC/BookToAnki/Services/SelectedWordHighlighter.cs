using System.Text;
using System.Text.RegularExpressions;

namespace BookToAnki.Services;

public class SelectedWordHighlighter
{
    public (string sentenceWithHighlight, string? lastHightlightedWord) HighlightWordInHtmlSentence(string htmlSentence, string plainTextWord)
    {
        string? lastHightlightedWord = null;
        // hack to force word boundary at the end of sentence:
        htmlSentence += " ";

        var plainTextLower = plainTextWord.ToLowerInvariant();

        StringBuilder rebuiltSentence = new StringBuilder();
        string wordCandidate = "";
        int numHtmlOpeningTags = 0;
        char previousCharacter = ' ';
        for (int i = 0; i < htmlSentence.Length; i++)
        {
            char c = htmlSentence[i];
            var nextCharacter = i < htmlSentence.Length - 1 ? htmlSentence[i + 1] : ' ';
            if (c == '<')
            {
                numHtmlOpeningTags++;
                wordCandidate += c;
            }
            else if (c == '>')
            {
                numHtmlOpeningTags--;
                wordCandidate += c;
            }
            else if (IsWordBoundary(c, numHtmlOpeningTags, previousCharacter, nextCharacter))
            {
                var wordCandidatePlainText = StripHtml(wordCandidate).ToLowerInvariant();
                if (wordCandidatePlainText == plainTextLower)
                {
                    rebuiltSentence.Append("<strong>").Append(wordCandidate).Append("</strong>");
                    lastHightlightedWord = wordCandidate;
                }
                else
                {
                    rebuiltSentence.Append(wordCandidate);
                }
                rebuiltSentence.Append(c);
                wordCandidate = "";
            }
            else
            {
                wordCandidate += c;
            }
            previousCharacter = c;
        }

        return (rebuiltSentence.ToString().Trim(), lastHightlightedWord);
    }

    private string StripHtml(string html)
    {
        return Regex.Replace(html, "<.*?>", string.Empty);
    }

    private static readonly char[] WordBoundaryCharacters = { ' ', '\t', '.', '!', '?', '…', ',', '"', '‑', '—', '-', ':', '(', ')', ';', '„', ',', '.', '|', '?', ' ', ']', '[' };
    private bool IsWordBoundary(char c, int numHtmlOpeningTags, char previousCharacter, char nextCharacter)
    {
        return numHtmlOpeningTags == 0 && (WordBoundaryCharacters.Contains(c)
            || (c == '\'' && WordBoundaryCharacters.Contains(previousCharacter))
            || (c == '\'' && WordBoundaryCharacters.Contains(nextCharacter))
            );
    }
}
