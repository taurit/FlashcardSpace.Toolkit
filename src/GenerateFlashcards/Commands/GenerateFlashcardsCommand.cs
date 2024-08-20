using CoreLibrary.Interfaces;
using GenerateFlashcards.Services;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;
using System.Diagnostics.CodeAnalysis;

namespace GenerateFlashcards.Commands;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global", Justification = "Instantiated by Spectre.Console.Cli when needed")]
internal sealed class GenerateFlashcardsCommand(
        ILogger<GenerateFlashcardsCommand> logger,
        BuildingBlocksProvider buildingBlocksProvider
    ) : AsyncCommand<GenerateFlashcardsCommandSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, GenerateFlashcardsCommandSettings settings)
    {
        logger.LogInformation("Generating flashcards...");
        logger.LogInformation("Settings:\n{@Settings}\n", settings);

        logger.LogInformation("Extracting words from input file...");

        IExtractSentences sentencesExtractor = buildingBlocksProvider.SelectBestSentenceExtractor(settings);
        var extractedSentences = await sentencesExtractor.ExtractSentences(settings.InputFilePath);
        logger.LogInformation("Extracted {ExtractedSentencesCount} sentences", extractedSentences.Count);
        logger.LogDebug("Sample of extracted sentences:\n{@SampleOfSentences}", extractedSentences.Take(3));

        logger.LogInformation("Extracting terms from the sentences...");
        IExtractTerms termExtractor = buildingBlocksProvider.SelectBestTermExtractor(settings);
        var extractedTerms = await termExtractor.ExtractTerms(extractedSentences);
        logger.LogDebug("Sample of extracted terms:\n{@SampleOfTerms}", extractedTerms.Take(3));

        logger.LogInformation("Translating terms...");
        return 0;
    }

}
