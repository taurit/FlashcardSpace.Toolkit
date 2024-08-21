using AdvancedSentenceExtractor.Models;
using BookToAnki.Models;

namespace BookToAnki.Services;

public static class SentenceMatcher
{
    private record EqualWordPair(String WordA, String WordB);
    private static readonly EqualWordPair[] EqualWordPairs = new EqualWordPair[] {
            new EqualWordPair( "і", "й" ),
            new EqualWordPair( "з", "із" ),
            new EqualWordPair( "в", "у" ),
            new EqualWordPair( "вже", "уже" ),
        };

    private static readonly Dictionary<string, string> DirectionA;
    private static readonly Dictionary<string, string> DirectionB;
    static SentenceMatcher()
    {
        DirectionA = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        DirectionB = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var wordPair in EqualWordPairs)
        {
            DirectionA.Add(wordPair.WordA, wordPair.WordB);
            DirectionB.Add(wordPair.WordB, wordPair.WordA);
        }
    }

    public static List<SentenceWithSound> Match(List<Sentence> sentences, Transcript transcript)
    {
        int lastFoundMatchIndex = 0;
        int lastFoundMatchLengthWords = 0;

        List<SentenceWithSound> result = new List<SentenceWithSound>();

        foreach (var sentence in sentences)
        {
            var wordsToMatch = sentence.Words;

            var wordCount = wordsToMatch.Count;
            var rangeAheadToScan = wordCount switch
            {
                _ when wordCount <= 3 => 100,
                _ => 200,
            };

            var rangeStart = lastFoundMatchIndex + lastFoundMatchLengthWords;
            var rangeEnd = Math.Min(rangeStart + rangeAheadToScan, transcript.TranscriptWords.Count - 1);

            var rangeToScan = new Range(rangeStart, rangeEnd);

            for (int candidateStart = rangeToScan.Start.Value; candidateStart < rangeToScan.End.Value; candidateStart++)
            {
                int transcribedWordShift = 0;
                int wordsSkippedFromTranscript = 0;
                bool matchFound = true;

                for (int wordToMatch = 0; wordToMatch < wordsToMatch.Count; wordToMatch++)
                {
                    string word = wordsToMatch[wordToMatch];
                    var candidateIndex = candidateStart + transcribedWordShift;

                    if (candidateIndex >= transcript.TranscriptWords.Count)
                    {
                        matchFound = false;
                        break;
                    }
                    var candidateMatch = transcript.TranscriptWords[candidateIndex];
                    if (!UkrainianWordEquals(candidateMatch.Word, word))
                    {
                        // heuristics: allow skip 1 word from transcription
                        var isFirstWordInSentence = wordToMatch == 0;
                        var isLastWordInSentence = wordToMatch == wordsToMatch.Count - 1;
                        if (!isFirstWordInSentence && !isLastWordInSentence)
                        {
                            var nextWordToMatch = wordsToMatch[wordToMatch + 1];
                            var nextCandidateIndex = candidateStart + transcribedWordShift + 2;
                            bool nextCandidateExists = nextCandidateIndex < transcript.TranscriptWords.Count;

                            if (wordsSkippedFromTranscript < 2 &&
                                !isLastWordInSentence &&
                                nextCandidateExists &&
                                UkrainianWordEquals(nextWordToMatch, transcript.TranscriptWords[nextCandidateIndex].Word)
                                )
                            {
                                transcribedWordShift += 2;
                                wordsSkippedFromTranscript++;
                            }
                            else
                            {
                                matchFound = false;
                            }
                        }
                        else
                        {
                            matchFound = false;
                            break;
                        }
                    }
                    else
                    {
                        transcribedWordShift++;
                    }
                }

                if (matchFound)
                {
                    var matchedWords = transcript.TranscriptWords.Skip(candidateStart).Take(wordsToMatch.Count + wordsSkippedFromTranscript).ToList();
                    var sentenceWithSound = new SentenceWithSound(sentence, matchedWords, transcript.AudioFilePath);
                    result.Add(sentenceWithSound);

                    lastFoundMatchIndex = candidateStart;
                    // move at least 1 word to not get stuck in a loop
                    lastFoundMatchLengthWords = sentence.Words.Count == 0 ? 1 : sentence.Words.Count;
                    break;
                }
            }
        }

        return result;
    }

    public static bool UkrainianWordEquals(string word1, string word2)
    {
        // optimization => if length already determines that words are not the same, skip expensive computation
        if (Math.Abs(word1.Length - word2.Length) > 2) return false;

        // equal words
        if (DirectionA.TryGetValue(word1, out string? value1) && value1.Equals(word2, StringComparison.InvariantCultureIgnoreCase))
            return true;
        if (DirectionB.TryGetValue(word1, out string? value2) && value2.Equals(word2, StringComparison.InvariantCultureIgnoreCase))
            return true;

        // generic comparer
        bool areEqual = word1.Equals(word2, StringComparison.InvariantCultureIgnoreCase);

        // heuristics: if not equal, but they are long words and are very close, assume error in transcription and accept the words as the same
        if (!areEqual && word1.Length > 3)
        {
            var likelyTheSameWord = StringDistance.AreStringsVerySimilar(word1.ToLowerInvariant(), word2.ToLowerInvariant());
            return likelyTheSameWord;
        }

        return areEqual;
    }

}
