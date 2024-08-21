using AdvancedSentenceExtractor.Models;
using AdvancedSentenceExtractor.Services;
using BookToAnki.Models;
using MemoryPack;

namespace BookToAnki.Services;
public class BookLoader
{
    private readonly UkrainianWordExplainer _ukrainianWordExplainer;
    private readonly WordTokenizer _wordTokenizer;
    private readonly SentenceFactory _sentenceBuilder;
    private readonly SentenceTokenizer _sentenceTokenizer;
    private readonly WordStatisticCounter _wordStatisticCounter;
    private readonly BilingualSentenceMatcher _bilingualSentenceMatcher;

    public BookLoader(UkrainianWordExplainer ukrainianWordExplainer)
    {
        _ukrainianWordExplainer = ukrainianWordExplainer;
        _wordTokenizer = new WordTokenizer();
        _sentenceBuilder = new SentenceFactory(_wordTokenizer);
        _sentenceTokenizer = new SentenceTokenizer(_sentenceBuilder);
        _wordStatisticCounter = new WordStatisticCounter(_ukrainianWordExplainer);
        _bilingualSentenceMatcher = new BilingualSentenceMatcher(_wordTokenizer);
    }

    public async Task<(List<WordOccurrences> wordsData, (BilingualSentenceMatchingResult enUk, BilingualSentenceMatchingResult enPl) sentenceMatches)> LoadWordsFromBook(SingleBookMetadata selectedBook, int numWordsInAGroup, string sentenceMatchesCacheFolder)
    {
        var allSentences = new List<Sentence>();
        var allSentencesWithSound = new List<SentenceWithSound>();

        var machineTranslationsUkPl = _bilingualSentenceMatcher
            .LoadSentencesWithMachineTranslations(selectedBook.SentencesUkPath, selectedBook.SentencesUkPlPath)
            .ToLookup(x => x.PrimaryLanguage)
            ;
        var machineTranslationsUkEn = _bilingualSentenceMatcher
                .LoadSentencesWithMachineTranslations(selectedBook.SentencesUkPath, selectedBook.SentencesUkEnPath)
                .ToLookup(x => x.PrimaryLanguage)
            ;

        foreach (var bookPart in selectedBook.Parts)
        {
            var bookContent = await File.ReadAllTextAsync(bookPart.OriginalTextPath);
            var sentences = _sentenceTokenizer.TokenizeBook(bookContent);

            allSentences.AddRange(sentences);

            var transcript = TranscriptReader.ReadTranscript(bookPart.GoogleTextToSpeechTranscriptPath, bookPart.AudioAbsolutePath);

            // match with sounds
            var sentencesWithSounds = SentenceMatcher.Match(sentences, transcript);
            allSentencesWithSound.AddRange(sentencesWithSounds);
        }

        var sentenceEquivalentsInDifferentLanguages = GetSentenceEquivalentsInDifferentLanguages(selectedBook, sentenceMatchesCacheFolder);

        var wordsData = await _wordStatisticCounter
            .GetWordsByFrequency(allSentences, allSentencesWithSound, numWordsInAGroup, sentenceEquivalentsInDifferentLanguages, machineTranslationsUkPl, machineTranslationsUkEn);

        return (wordsData, sentenceEquivalentsInDifferentLanguages);
    }

    private (BilingualSentenceMatchingResult enUk, BilingualSentenceMatchingResult enPl) GetSentenceEquivalentsInDifferentLanguages(SingleBookMetadata selectedBook, string sentenceMatchesCacheFolder)
    {

        var cacheFileName = Path.Combine(sentenceMatchesCacheFolder, $"{selectedBook.BookTitle.Split("\\").Last()}.matchCache.bin");

        if (File.Exists(cacheFileName))
        {
            var cacheContent = File.ReadAllBytes(cacheFileName);
            var cached = MemoryPackSerializer.Deserialize<BilingualMatchingResultMempack>(cacheContent);
            if (cached is null)
            {
                throw new FileLoadException($"Cache file `{cacheFileName}` seems broken, remove it!");
            }
            return (cached.EnUk, cached.EnPl);
        }


        var enUkSentences = _bilingualSentenceMatcher.LoadSentencesWithMachineTranslations(
            selectedBook.SentencesEnPath,
            selectedBook.SentencesEnUkPath
        );

        var ukEnSentences = _bilingualSentenceMatcher.LoadSentencesWithMachineTranslations(
            selectedBook.SentencesUkPath,
            selectedBook.SentencesUkEnPath
        );

        var enPlSentences = _bilingualSentenceMatcher.LoadSentencesWithMachineTranslations(
            selectedBook.SentencesEnPath,
            selectedBook.SentencesEnPlPath
        );

        var plEnSentences = _bilingualSentenceMatcher.LoadSentencesWithMachineTranslations(
            selectedBook.SentencesPlPath,
            selectedBook.SentencesPlEnPath
        );

        var enUkSentenceMatch = _bilingualSentenceMatcher.Match(enUkSentences, ukEnSentences);
        var enPlSentenceMatch = _bilingualSentenceMatcher.Match(enPlSentences, plEnSentences);

        var bin = MemoryPackSerializer.Serialize(new BilingualMatchingResultMempack(enUkSentenceMatch, enPlSentenceMatch));
        File.WriteAllBytes(cacheFileName, bin);

        return (enUkSentenceMatch, enPlSentenceMatch);
    }

    public List<Sentence> LoadSentencesFromBook(string fullBookTxtPath)
    {
        var bookContent = File.ReadAllText(fullBookTxtPath);
        var allSentences = _sentenceTokenizer.TokenizeBook(bookContent);

        return allSentences;
    }

}
