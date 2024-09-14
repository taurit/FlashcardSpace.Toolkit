using CoreLibrary;
using CoreLibrary.Services;
using CoreLibrary.Services.ObjectGenerativeFill;
using GenerateFlashcards.Models;
using GenerateFlashcards.Models.Spanish;

namespace GenerateFlashcards.Services;
public class FrequencyDictionaryTermExtractor(GenerativeFill generativeFill, NormalFormProvider normalFormProvider) : IExtractTerms
{
    public async Task<List<TermInContext>> ExtractTerms(
        string inputFileName,
        SupportedInputLanguage sourceLanguage,
        int numItemsToSkip,
        int numItemsToTake
    )
    {
        List<TermInContext>? extractedTerms;

        // Hints for ChatGPT model vary depending on the language.
        // I think a generic implementation working with several languages would be more complex overall.
        if (sourceLanguage == SupportedInputLanguage.Spanish)
            extractedTerms = await ExtractTermsFromSpanishFrequencyDictionary(inputFileName, numItemsToSkip, numItemsToTake);
        else
            throw new NotImplementedException("Only Spanish language is supported at the moment by the FrequencyDictionaryTermExtractor.");

        return extractedTerms;
    }

    private async Task<List<TermInContext>> ExtractTermsFromSpanishFrequencyDictionary(string inputFileName, int numItemsToSkip, int numItemsToTake)
    {
        var frequencyDictionary = new FrequencyDataProvider(normalFormProvider, inputFileName);

        var wordsToAnalyze = frequencyDictionary
            .Take(numItemsToSkip, numItemsToTake)
            .Select(record => new SpanishWordPartsOfSpeech { IsolatedWord = record.Term }).ToList();

        var words = await generativeFill
            .FillMissingProperties(Parameters.OpenAiModelId, Parameters.OpenAiModelClassId, wordsToAnalyze);

        List<TermInContext> terms = new List<TermInContext>();

        foreach (var word in words)
        {
            foreach (var possiblePartOfSpeech in word.PossiblePartsOfSpeechUsage)
            {
                PartOfSpeech partOfSpeechMapped = possiblePartOfSpeech.PartOfSpeech.ToCorePartOfSpeech();
                var term = new TermInContext(word.IsolatedWord,
                        possiblePartOfSpeech.WordBaseForm,
                        possiblePartOfSpeech.SentenceExample,
                        partOfSpeechMapped
                    );
                terms.Add(term);
            }

        }

        return terms;
    }
}

