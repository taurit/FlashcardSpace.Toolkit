using BookToAnki.Models;
using CoreLibrary.Interfaces;
using CoreLibrary.Services;

namespace BookToAnki.Services;

/// <summary>
///     Matches sentences in two human-translated ebooks in two different languages, to get a quality translation examples
/// </summary>
public class BilingualSentenceMatcher(IWordTokenizer wordTokenizer)
{
    private const int LookAheadWindowSentences = 30; // 30 as baseline, works well.  50 too much, breaks matching. 20 too little. 35 and 32 worse than 30. 28 breaks 1 book. 29 is perfect, better than 30 and the optimum value.
    private const int LookBackWindowWords = 12; // 10 is proven better than 7, and 7 proven better than 5. 12 is still better than 10. 15 breaks one book matching so it's too much.

    public BilingualSentenceMatchingResult Match(
        List<BilingualSentence> sentencesInPrimaryLanguage,
        List<BilingualSentence> sentencesInSecondaryLanguage)
    {
        var startingIndexOfNextCandidate = 0;
        var matchingSentences = new List<BilingualSentence>();

        // sometimes a book has a fragment or poem or other rhymed stuff that
        // typically fails automated matching. This is an attempt to skip such fragments,
        // instead of failing to match the rest of the book
        int numConsecutiveFailedMatches = 0;

        foreach (var s1 in sentencesInPrimaryLanguage)
        {
            var candidateRangeFirst = Math.Max(0, startingIndexOfNextCandidate - LookBackWindowWords);
            var candidateRangeLast =
                Math.Min(startingIndexOfNextCandidate + LookAheadWindowSentences + numConsecutiveFailedMatches, sentencesInSecondaryLanguage.Count - 1);
            var matchFound = false;
            for (var candidateIndex = candidateRangeFirst; candidateIndex <= candidateRangeLast; candidateIndex++)
            {
                var s2 = sentencesInSecondaryLanguage[candidateIndex];
                if (SentenceMatches(s1.PrimaryLanguage, s2.SecondaryLanguage) ||
                    SentenceMatches(s1.SecondaryLanguage, s2.PrimaryLanguage)
                   )
                {
                    matchingSentences.Add(new BilingualSentence(s1.PrimaryLanguage, s2.PrimaryLanguage));
                    startingIndexOfNextCandidate = candidateIndex + 1;
                    matchFound = true;
                    numConsecutiveFailedMatches = 0;
                    break;
                }
            }

            if (!matchFound)
            {
                numConsecutiveFailedMatches++;
            }
        }

        var successRate = 100f * matchingSentences.Count / sentencesInPrimaryLanguage.Count;
        var result = new BilingualSentenceMatchingResult(matchingSentences, successRate);

        return result;
    }

    private static List<string> _ignoredWords = new List<string>
    {
        "the", "a", "to", "me"
    };

    public bool SentenceMatches(string sentence1, string sentence2)
    {
        if (sentence1 == sentence2) return true;

        if (!_lowercaseWordCache.ContainsKey(sentence1))
        {
            _lowercaseWordCache[sentence1] = wordTokenizer
                .GetWords(sentence1)
                .Except(_ignoredWords, StringComparer.InvariantCultureIgnoreCase)
                .Select(x => x.ToLowerInvariant())
                .ToList();
        }

        if (!_lowercaseWordCache.ContainsKey(sentence2))
            _lowercaseWordCache[sentence2] = wordTokenizer
                .GetWords(sentence2)
                .Except(_ignoredWords)
                .Select(x => x.ToLowerInvariant())
                .ToList();

        var sentence1Words = _lowercaseWordCache[sentence1];
        var sentence2Words = _lowercaseWordCache[sentence2];

        var s1 = new Sentence(sentence1, sentence1Words);
        var s2 = new Sentence(sentence2, sentence2Words);

        var numWordsS1 = s1.Words.Count;
        var numWordsS2 = s2.Words.Count;
        float numCommonWords = s1.Words.Intersect(s2.Words).Count();

        var commonWordRatioS1 = numCommonWords / numWordsS1;
        var commonWordRatioS2 = numCommonWords / numWordsS2;

        if (numWordsS1 > 6 && commonWordRatioS1 > 0.42 && commonWordRatioS2 > 0.42)
        {
            return true;
        }

        if (numWordsS1 <= 6 && commonWordRatioS1 > 0.55 && commonWordRatioS2 > 0.55)
        {
            return true;
        }

        return false;
    }

    public List<BilingualSentence> LoadSentencesWithMachineTranslations(string fileNameOriginals, string fileNameTranslations)
    {
        var sentencesL1Human = File.ReadAllLines(fileNameOriginals);
        var sentencesL1ToL2 = File.ReadAllLines(fileNameTranslations);

        if (sentencesL1Human.Length != sentencesL1ToL2.Length)
            throw new ArgumentException("Number of lines of original sentences and machine translated sentences does not match.");

        var bookContent = new List<BilingualSentence>(sentencesL1Human.Length);
        for (int i = 0; i < sentencesL1Human.Length; i++)
        {
            var bilingualSentence = new BilingualSentence(sentencesL1Human[i], sentencesL1ToL2[i]);
            bookContent.Add(bilingualSentence);
        }
        return bookContent;
    }

    readonly Dictionary<string, List<string>> _lowercaseWordCache = new(130000);
}
