using BookToAnki.Models;
using BookToAnki.NotePropertiesDatabase;
using System.Diagnostics;

namespace BookToAnki.Services;

public class AnkiNoteGenerator
{
    private readonly AudioExampleProvider _audioExampleProvider;
    private readonly AudioSampleSelector _audioSampleSelector;
    private readonly NoteProperties _noteProperties;
    private readonly SelectedWordHighlighter _selectedWordHighlighter;
    private readonly UkrainianStressHighlighterWithCache _ukrainianStressHighlighter;
    private readonly UkrainianWordExplainer _ukrainianWordExplainer;

    public AnkiNoteGenerator(UkrainianStressHighlighterWithCache ukrainianStressHighlighter,
        AudioExampleProvider audioExampleProvider,
        SelectedWordHighlighter selectedWordHighlighter,
        UkrainianWordExplainer ukrainianWordExplainer,
        AudioSampleSelector audioSampleSelector,
        NoteProperties noteProperties)
    {
        _ukrainianStressHighlighter = ukrainianStressHighlighter;
        _audioExampleProvider = audioExampleProvider;
        _selectedWordHighlighter = selectedWordHighlighter;
        _ukrainianWordExplainer = ukrainianWordExplainer;
        _audioSampleSelector = audioSampleSelector;
        _noteProperties = noteProperties;
    }

    public async Task PrimeStressCachesForGeneratingAnkiNotes(List<WordUsageExample> wordUsages)
    {
        // fixed: a batch of messages might contain duplicate example (same example used to describe multiple words).
        var examples = wordUsages.DistinctBy(x => x.Sentence.Text).ToList();

        var counter = 0;
        foreach (var example in examples)
        {
            counter++;
            if (_ukrainianStressHighlighter.AreStressesCachedAlready(example.Sentence.Text))
                throw new ArgumentException(
                    $"The following sentence is already in cache, so the client should not ask to prime cache for it to avoid repeatedly asking to process the same data: {example.Sentence.Text}.");

            var sentenceWithStressesHighlighted =
                await _ukrainianStressHighlighter.HighlightStresses(example.Sentence.Text);
            Debug.WriteLine(
                $"Fetching ukrainian stress ({counter}/{examples.Count}). Sentence: {example.Sentence.Text}...");
        }

        _ukrainianStressHighlighter.SaveCache();
    }


    public async Task PrimeAudioCachesForGeneratingAnkiNotes(List<WordUsageExample> wordUsages)
    {
        var padding = TimeSpan.FromSeconds(2);
        var shift = new AudioShift(-padding, padding);

        var examples = wordUsages.DistinctBy(x => x.Sentence.Text).ToList();
        await Parallel.ForEachAsync(examples, async (row, ct) =>
        {
            try
            {
                Debug.WriteLine($"Priming audio cache for sentence: {row.Sentence.Text}");
                var bestAudioMatch = _audioSampleSelector.TrySelectBestAudioSample(row);

                if (bestAudioMatch is null)
                    return;

                var audioExample = await _audioExampleProvider.GenerateAudioSample(bestAudioMatch);
                var audioExampleForEditing = await _audioExampleProvider.GenerateAudioSample(bestAudioMatch, shift);
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Exception in async code: {e}");
            }
        });

        Debug.WriteLine("Done priming audio cache.");
    }


    public async Task<UkrainianAnkiNote> GenerateAnkiNote(WordUsageExample wordUsage, string? imageFolderPath)
    {
        var word = wordUsage.Word;
        var matchInAudioTranscript = _audioSampleSelector.TrySelectBestAudioSample(wordUsage);

        var sentenceWithStressesHighlighted =
            await _ukrainianStressHighlighter.HighlightStresses(matchInAudioTranscript.Sentence.Text);

        var highlights =
            _selectedWordHighlighter.HighlightWordInHtmlSentence(sentenceWithStressesHighlighted, word);

        var key = new PrefKey(wordUsage.Word, wordUsage.Sentence.Text);
        var customPadding = _noteProperties.GetAudioSampleShift(key);
        var audioExample = await _audioExampleProvider.GenerateAudioSample(matchInAudioTranscript, customPadding);

        // todo I might want to ensure explanation exists here (e.g. send prompt, or open window for manual prompt)
        //var explanation = await _ukrainianWordExplainer.Explain(new WordToExplain(word, wordUsage.Sentence.Text));

        var imageFileName = GetImageFileName(imageFolderPath);
        var imagePath = imageFolderPath is not null && imageFileName is not null
            ? Path.Combine(imageFolderPath, imageFileName)
            : null;

        var wordScopedKey = new PrefKey(wordUsage.Word, "*");
        var status = _noteProperties.GetNoteRating(key);

        var explanation = _ukrainianWordExplainer.TryGetExplanation(key, wordScopedKey);

        var nominativeFormHighlighted =
            await _ukrainianStressHighlighter.HighlightStresses(explanation.NominativeForm);
        if (String.IsNullOrWhiteSpace(nominativeFormHighlighted))
        {
            // for whatever reason, the highlighting service (3rd party) returns empty string for single-letter words, and maybe some others too
            nominativeFormHighlighted = explanation.NominativeForm;
        }

        var sentenceHumanTranslationEnglish =
            _noteProperties.GetSentenceTranslationEn(key) ?? wordUsage.SentenceHumanTranslationEnglish;

        var sentenceHumanTranslationPolish =
            _noteProperties.GetSentenceTranslationPl(key) ?? wordUsage.SentenceHumanTranslationPolish;

        var ankiCard = new UkrainianAnkiNote(
            key,
            highlights.lastHightlightedWord ?? word,
            highlights.sentenceWithHighlight,
            audioExample,
            explanation.NominativeForm,
            nominativeFormHighlighted,
            explanation.PolishTranslation,
            explanation.ExplanationInPolish,
            explanation.EnglishTranslation,
            explanation.ExplanationInEnglish,
            sentenceHumanTranslationEnglish,
            sentenceHumanTranslationPolish,
            imagePath,
            matchInAudioTranscript,
            wordUsage.TranscriptMatches,
            status,
            wordUsage.SentenceMachineTranslationPolish
        );
        return ankiCard;
    }


    private string? GetImageFileName(string? imageFolderPath)
    {
        if (imageFolderPath is null) return null;
        var preferenceFileName = Path.Combine(imageFolderPath, "preference.txt");
        if (File.Exists(preferenceFileName))
        {
            var preferredImage = File.ReadAllText(preferenceFileName);
            return preferredImage;
        }

        if (Directory.Exists(imageFolderPath))
        {
            var candidates = Directory.EnumerateFiles(imageFolderPath, "*.100.webp").ToList();
            if (candidates.Count == 1) return Path.GetFileName(candidates.First());
        }

        return null;
    }
}
