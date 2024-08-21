using BookToAnki.Services.OpenAi;
using Fastenshtein;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace BookToAnki.Services;

public record UkrainianWordSimilarity(bool IsPossiblyTheSameWord, double EmbeddingsDistance);
public record SimilarWord(string Word, double Similarity);

public class UkrainianWordSimilarityEvaluator(EmbeddingsServiceWrapper embeddingsService)
{
    private readonly EmbeddingsServiceWrapper _embeddingsService = embeddingsService;

    readonly ConcurrentDictionary<string, string> _wordToStemCache = new ConcurrentDictionary<string, string>();

    public async Task PrimeCache(List<string> words)
    {
        await _embeddingsService.PrimeEmbeddingsCache(words);
    }

    private bool IsPossiblyTheSameWord(string word1, string word2)
    {
        // Algorithm 1: rule-based
        var stem1 = GetStem(word1);
        var stem2 = GetStem(word2);
        bool wordsSeemRelatedUsingRules = word1.Length >= 4 && stem1 == stem2;

        // Algorithm 2: raw distance
        bool wordsAreVeryClose = word1.Length >= 3 && Levenshtein.Distance(word1, word2) < 3;

        // Algorithm 3: shared prefix
        bool wordsShareLongPrefix = word1.Length > 5 && word2.Length > 5 && word1.Substring(0, 4) == word2.Substring(0, 4);

        var isPossiblyTheSameWord = wordsSeemRelatedUsingRules || wordsAreVeryClose || wordsShareLongPrefix;

        return isPossiblyTheSameWord;
    }

    private string GetStem(string word)
    {
        if (_wordToStemCache.TryGetValue(word, out var stem)) return stem;

        var guessedBaseForm = word
            .ToLower()
            .ReplaceEnding("сь", "ся")

            // reflexive verbs are quite distinctive (e.g., нахилюся)
            .ReplaceEnding("ився", "итися.") // past, masculine
            .ReplaceEnding("лилися", "литися.") // past, plural
            .ReplaceEnding("илася", "итися.") // past, feminine
            .ReplaceEnding("юся", "итися.") // future, 1p 
            .ReplaceEnding("имося", "итися.") // future, 1p
            .ReplaceEnding("имся", "итися.") // future, 1p
            .ReplaceEnding("ишся", "итися.") // future, 2p
            .ReplaceEnding("ишся", "итися.") // future, 2p
            .ReplaceEnding("итеся", "итися.") // future, 2p
            .ReplaceEnding("иться", "итися.") // future, 3p
            .ReplaceEnding("яться", "итися.") // future, 3p
            .ReplaceEnding("іться", "итися.") // imperative
            .ReplaceEnding("ися", "итися.") // not sure, future maybe
                                            //.ReplaceEnding("ися", "итися.") // imperative // might be often incorrect, short and often appears at the end

            // but the rest is tricky, so I'll attempt to just remove whatever ending they have and leave stem.

            // adjectives (e.g., теплий)

            // Добрий (Dobryi) - Good
            // Молодий (Molodyi) - Young
            // Дорогий (Dorohyi) - Expensive
            // Гарячий (Haryachyi) - Hot
            // Гіркий (Hirkyi) - Bitter
            // Круглий (Kruhlyi) - Round
            // Чистий (Chystyi) - Clean
            // Новий (Novyi) - New
            // Інтересний (Interesnyi) - Interesting

            // past tense like пішов, робив
            .ReplaceEndingRegex("[аи]ти", ".")
            .ReplaceEndingRegex("[аи]в", ".")
            .ReplaceEndingRegex("[аи]ла", ".")
            .ReplaceEndingRegex("[аи]ло", ".")
            .ReplaceEndingRegex("[аи]ли", ".")
            .ReplaceEndingRegex("шов", ".")
            .ReplaceEndingRegex("шла", ".")
            .ReplaceEndingRegex("шло", ".")
            .ReplaceEndingRegex("шли", ".")

            // nouns, like будинок
            .ReplaceEndingRegex("нок", ".")
            .ReplaceEndingRegex("нки", ".")
            .ReplaceEndingRegex("нку", ".")
            .ReplaceEndingRegex("нком", ".")
            .ReplaceEndingRegex("нкам", ".")
            .ReplaceEndingRegex("нків", ".")
            .ReplaceEndingRegex("нкові", ".")
            .ReplaceEndingRegex("нками", ".")
            .ReplaceEndingRegex("нках", ".")

            // adverbs (e.g., тепло)
            .ReplaceEnding("ло", ".")
            .ReplaceEndingRegex("([лрдгчклтвн])ого", ".")
            .ReplaceEndingRegex("([лрдгчклтвн])ому", ".")
            .ReplaceEndingRegex("([лрдгчклтвн])им", ".")
            .ReplaceEndingRegex("([лрдгчклтвн])ої", ".")
            .ReplaceEndingRegex("([лрдгчклтвн])ій", ".")
            .ReplaceEndingRegex("([лрдгчклтвн])ім", ".")
            .ReplaceEndingRegex("([лрдгчклтвн])ий", ".")
            //.ReplaceEndingRegex("([лрдгчклтвн])не", ".")
            .ReplaceEndingRegex("([лрдгчклтвн])ою", ".")
            .ReplaceEndingRegex("([лрдгчклтвн])их", ".")
            .ReplaceEndingRegex("([лрдгчклтвн])ими", ".")
            .ReplaceEndingRegex("([лрдгчклтвн])ом", ".")
            .ReplaceEndingRegex("([лрдгчклтвн])у", ".")
            .ReplaceEndingRegex("([лрдгчклтвн])і", ".")
            .ReplaceEndingRegex("([лрдгчклтвн])а", ".") // "$1ий." not great in this case. Maybe i just cut the suffix?
            .ReplaceEndingRegex("([лрдгчклтвн])o", ".")
            .ReplaceEndingRegex("([лрдгчклтвн])е", ".")
            .ReplaceEnding(".",
                ""); // dot is a hack to ensure that no other replacements would follow the initial replacement


        // for short words, probability of a mistake is just very high. And I'd rather have less groups, but be sure of their quality.
        //if (guessedBaseForm.Length < 2) guessedBaseForm = word;
        _wordToStemCache.TryAdd(word, guessedBaseForm);
        return guessedBaseForm;
    }

    public async Task<double> CalculateSimilarity(string word1, string word2)
    {
        var word1Embedding = await _embeddingsService.CreateEmbedding(word1);
        var word2Embedding = await _embeddingsService.CreateEmbedding(word2);

        var similarity = _embeddingsService.CosineSimilarity(word1Embedding, word2Embedding);
        return similarity;
    }

    public async Task<IReadOnlyCollection<SimilarWord>> FindMostSimilarWords(IReadOnlyList<string> givenWords, List<string> candidates)
    {
        var similarWords = new ConcurrentBag<SimilarWord>();

        // only makes sense if there is a risk of missing one of them by heuristic filtering later:
        //foreach (var givenWord in givenWords)
        //{
        //    similarWords.Add(new SimilarWord(givenWord, 1));
        //}
        Stopwatch s = Stopwatch.StartNew();
        await Parallel.ForEachAsync(candidates, async (candidate, token) =>
        {
            var candidateEmbedding = await _embeddingsService.CreateEmbedding(candidate);

            double maxSimilarity = 0;
            foreach (var givenWord in givenWords)
            {
                var givenEmbedding = await _embeddingsService.CreateEmbedding(givenWord);

                var similarity = _embeddingsService.CosineSimilarity(candidateEmbedding, givenEmbedding);
                maxSimilarity = Math.Max(maxSimilarity, similarity);
            }

            var similarWord = new SimilarWord(candidate, maxSimilarity);

            if (similarWord.Similarity > 0.915)
            {
                similarWords.Add(similarWord);
            }
            else
            {
                // I see no value, Embedding are good enough, and this mostly adds unrelated words differing by 1-2 letters.
                //bool heuristicSuggestsRelatedWordDespiteLowSimilarity = givenWords.All(gw => IsPossiblyTheSameWord(gw, similarWord.Word));
                //if (heuristicSuggestsRelatedWordDespiteLowSimilarity)
                //{
                //    similarWords.Add(similarWord);
                //}
            }
        });
        s.Stop();
        Debug.WriteLine($"FindMostSimilarWords: {s.ElapsedMilliseconds}");

        return similarWords;
    }
}
