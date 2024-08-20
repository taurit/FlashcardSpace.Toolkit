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
        logger.LogInformation("Settings:\n\n{@Settings}\n", settings);

        logger.LogInformation("Extracting words from input file...");

        IExtractWords wordsExtractor = buildingBlocksProvider.SelectBestWordExtractor(settings);
        var extractedWords = await wordsExtractor.ExtractWords(settings.InputFilePath);
        logger.LogInformation("Extracted {ExtractedWordsCount} words", extractedWords.Count);
        logger.LogDebug("Sample of extracted words:\n\n{@FewWords}", extractedWords.Take(3));

        logger.LogInformation("Determining parts of speech for the extracted words...");
        IExtendNotes partOfSpeechClassifier = buildingBlocksProvider.SelectBestPartOfSpeechClassifier(settings);
        //partOfSpeechClassifier.

        return 0;
    }

}
