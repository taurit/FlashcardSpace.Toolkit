using CoreLibrary.Services.GenerativeAiClients;
using GenerateFlashcards.Services;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;
using System.Diagnostics.CodeAnalysis;

namespace GenerateFlashcards.Commands;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global", Justification = "Instantiated by Spectre.Console.Cli when needed")]
internal sealed class GenerateFlashcardsCommand(
        ILogger<GenerateFlashcardsCommand> logger,
        BuildingBlocksProvider buildingBlocksProvider,
        IGenerativeAiClient chatGptClient
    ) : AsyncCommand<GenerateFlashcardsCommandSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, GenerateFlashcardsCommandSettings settings)
    {

        return 0;

        var sentences = await ExtractSentences(settings);
        var terms = await ExtractTerms(settings, sentences);
        var termsWithTranslations = await TranslateTerms(settings, terms);

        // chatgpt test
        var response = await chatGptClient.GetAnswerToPrompt(
            "gpt-4o-mini",
            "gpt-4o-mini",
            "You are a helpful assistant",
            "What is the capital of Ostrołęka?",
            false);
        logger.LogInformation($"ChatGPT response: {response}");

        return 0;
    }

    private async Task<List<string>> ExtractSentences(GenerateFlashcardsCommandSettings settings)
    {
        logger.LogInformation("Extracting sentences from input file {InputFileName}...", settings.InputFilePath);

        IExtractSentences sentencesExtractor = buildingBlocksProvider.SelectBestSentenceExtractor(settings);
        var extractedSentences = await sentencesExtractor.ExtractSentences(settings.InputFilePath);

        logger.LogInformation("Extracted {ExtractedSentencesCount} sentences", extractedSentences.Count);
        return extractedSentences;
    }

    private async Task<List<Note>> ExtractTerms(GenerateFlashcardsCommandSettings settings, List<string> extractedSentences)
    {
        logger.LogInformation("Extracting terms from the sentences...");

        IExtractTerms termExtractor = buildingBlocksProvider.SelectBestTermExtractor(settings);
        var extractedTerms = await termExtractor.ExtractTerms(extractedSentences, settings.InputLanguage.ToString());

        logger.LogInformation("Extracted {ExtractedTermsCount} terms", extractedTerms.Count);
        return extractedTerms;
    }


    private async Task<List<Note>> TranslateTerms(GenerateFlashcardsCommandSettings settings, List<Note> terms)
    {
        logger.LogInformation("Translating terms from {InputLanguage} to {OutputLanguage}...", settings.InputLanguage, settings.OutputLanguage);

        IProvideFieldValues translator = buildingBlocksProvider.SelectBestTranslator(settings);
        var translatedTerms = await translator.ProcessNotes(terms);

        logger.LogDebug("Sample of terms after translation:\n\n{@TranslatedTermsSample}", translatedTerms.Take(3));
        return translatedTerms;
    }
}
