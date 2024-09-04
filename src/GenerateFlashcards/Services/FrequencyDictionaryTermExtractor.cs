using CoreLibrary;
using CoreLibrary.Services.ObjectGenerativeFill;
using GenerateFlashcards.Models;

namespace GenerateFlashcards.Services;
public class FrequencyDictionaryTermExtractor(GenerativeFill generativeFill) : IExtractTerms
{
    public async Task<List<TermInContext>> ExtractTerms(List<string> extractedSentences, string contentInputLanguage)
    {
        var notes = new List<TermInContext>();

        // when working with a frequency dictionary, the sentences are just a list of words without a context.
        var words = extractedSentences;

        // so the optimal way to work with frequency dictionary is to learn top N word *families* 
        // source: https://www.scotthyoung.com/blog/2024/04/02/learn-vocabulary-language/


        var wordsToFill = words.Select(word => new EnglishWordInContext { Word = word }).ToList();
        var wordsFilled = await generativeFill.FillMissingProperties(Parameters.OpenAiModelId, Parameters.OpenAiModelClassId, wordsToFill);

        foreach (var word in wordsFilled)
        {
            PartOfSpeech partOfSpeechMapped = word.PartOfSpeech.ToCorePartOfSpeech();
            var note = new TermInContext(word.Word, word.WordBaseForm, word.SentenceExample, partOfSpeechMapped);
            notes.Add(note);
        }

        return notes;
    }
}

