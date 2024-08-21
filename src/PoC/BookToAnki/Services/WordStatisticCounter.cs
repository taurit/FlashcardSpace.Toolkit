using AdvancedSentenceExtractor.Models;
using BookToAnki.Models;

namespace BookToAnki.Services;

public class WordStatisticCounter(UkrainianWordExplainer ukrainianWordExplainer)
{
    private readonly UkrainianWordExplainer? _ukrainianWordExplainer = ukrainianWordExplainer;

    public Task<List<WordOccurrences>> GetWordsByFrequency(List<Sentence> sentences,
        List<SentenceWithSound> allSentencesWithSound,
        int numWords,
        (BilingualSentenceMatchingResult enUk, BilingualSentenceMatchingResult enPl) sentenceMatches,
        ILookup<string, BilingualSentence> machineTranslationsUkPl,
        ILookup<string, BilingualSentence> machineTranslationsUkEn)
    {
        var wordToContext = new Dictionary<string, List<WordUsageExample>>(StringComparer.InvariantCultureIgnoreCase);
        var sentencesToSounds = allSentencesWithSound.ToLookup(x => x.Sentence.Text);

        foreach (var sentence in sentences)
        {
            var wordGroups = GetWordGroups(sentence, numWords);

            var sentenceEquivalentEn = sentenceMatches.enUk.MatchedSentencesLookupReverse[sentence.Text].FirstOrDefault();
            var sentenceEquivalentPl = sentenceEquivalentEn is null ? null : sentenceMatches.enPl.MatchedSentencesLookup[sentenceEquivalentEn].FirstOrDefault();

            foreach (var wordGroup in wordGroups)
            {
                // perf optimization: if there is no sound sample for the sentence, reject it already at this point.
                if (!sentencesToSounds.Contains(sentence.Text)) continue;

                var sentenceWithSounds = sentencesToSounds[sentence.Text].ToList();

                // totally not guaranteed at this point, unless we cached it in previous app runs, but is useful as a ranking factor
                // for quality
                UkrainianWordExplanation? explanation = null;
                if (_ukrainianWordExplainer is not null)
                {
                    explanation = _ukrainianWordExplainer.TryGetExplanation(
                        new NotePropertiesDatabase.PrefKey(wordGroup, sentence.Text),
                        new NotePropertiesDatabase.PrefKey(wordGroup, "*")
                        );
                }

                var wordUsageContext = new WordUsageExample(
                    wordGroup,
                    sentence,
                    sentenceWithSounds,
                    machineTranslationsUkPl[sentence.Text].FirstOrDefault()?.SecondaryLanguage,
                    machineTranslationsUkEn[sentence.Text].FirstOrDefault()?.SecondaryLanguage,
                    sentenceEquivalentPl,
                    sentenceEquivalentEn,
                    explanation?.PolishTranslation
                );

                if (!wordToContext.ContainsKey(wordGroup))
                    wordToContext.Add(wordGroup, new List<WordUsageExample>());

                wordToContext[wordGroup].Add(wordUsageContext);
            }
        }

        return Task.FromResult(wordToContext.Select(x => new WordOccurrences(x.Key, x.Value)).ToList());
    }



    public List<string> GetWordGroups(Sentence sentence, int groupSize)
    {
        var wordGroups = new List<string>();
        for (int i = 0; i <= sentence.Words.Count - groupSize; i++)
        {
            var group = string.Join(" ", sentence.Words.GetRange(i, groupSize));
            wordGroups.Add(group);
        }

        return wordGroups;
    }
}
