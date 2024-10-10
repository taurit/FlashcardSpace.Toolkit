using CoreLibrary;
using CoreLibrary.Services;
using CoreLibrary.Services.ObjectGenerativeFill;
using GenerateFlashcards.Models.Spanish;
using Microsoft.Extensions.Logging;

namespace GenerateFlashcards.Services;
public class FrequencyDictionaryTermExtractor(
    GenerativeFill generativeFill,
    StringSanitizer stringSanitizer,
    ILogger<FrequencyDictionaryTermExtractor> logger
    )
{
    public async Task<List<TermInContext>> ExtractTerms(string inputFileName,
        SupportedLanguage sourceLanguage,
        PartOfSpeech? partOfSpeechFilter,
        int numItemsToSkip,
        int numItemsToTake)
    {
        logger.LogInformation("Extracting terms from frequency dictionary file {InputFileName}", inputFileName);
        logger.LogInformation("Source language: {SourceLanguage}", sourceLanguage);
        logger.LogInformation("Number of items to skip: {NumItemsToSkip}", numItemsToSkip);
        logger.LogInformation("Number of items to take: {NumItemsToTake}", numItemsToTake);

        List<TermInContext>? extractedTerms;

        // Hints for ChatGPT model vary depending on the language.
        // I think a generic implementation working with several languages would be more complex overall.
        if (sourceLanguage == SupportedLanguage.Spanish)
            extractedTerms = await ExtractTermsFromSpanishFrequencyDictionary(inputFileName, partOfSpeechFilter, numItemsToSkip, numItemsToTake);
        else
            throw new NotImplementedException("Only Spanish language is supported at the moment by the FrequencyDictionaryTermExtractor.");

        return extractedTerms;
    }

    private async Task<List<TermInContext>> ExtractTermsFromSpanishFrequencyDictionary(string inputFileName,
        PartOfSpeech? partOfSpeechFilter, int numItemsToSkip, int numItemsToTake)
    {
        var frequencyDictionary = new FrequencyDataProvider(stringSanitizer, inputFileName);

        List<TermInContext> terms = new List<TermInContext>();

        if (partOfSpeechFilter == null || partOfSpeechFilter == PartOfSpeech.Adjective)
        {
            var potentialAdjectives = frequencyDictionary
                .Take(numItemsToSkip, numItemsToTake)
                .Select(record => new SpanishAdjectiveDetector { IsolatedWord = record.Term }).ToList();

            var potentialAdjectivesAnalyzed = await generativeFill
                .FillMissingProperties(Parameters.OpenAiModelId, Parameters.OpenAiModelClassId, potentialAdjectives);

            var adjectivesAsTerms = potentialAdjectivesAnalyzed
                .Where(x => x.IsAdjective)
                .Select(x => new TermInContext(x.IsolatedWord, x.BaseForm!, x.SentenceExample, PartOfSpeech.Adjective));

            terms.AddRange(adjectivesAsTerms);
        }

        // todo Verbs, Nouns

        return terms;
    }
}

