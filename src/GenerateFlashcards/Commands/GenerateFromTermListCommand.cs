using CoreLibrary.Models;
using CoreLibrary.Services;
using GenerateFlashcards.Services;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;

namespace GenerateFlashcards.Commands;

internal sealed class GenerateFromTermListCommand(
        GenericSpanishTermExtractor genericSpanishTermExtractor,
        DeckExporter deckExporter,
        ImageProvider imageProvider,
        AudioProvider audioProvider,
        QualityControlService qualityControlService,
        ILogger<GenerateFromTermListCommand> logger
    ) : AsyncCommand<GenerateFromTermListCommandSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, GenerateFromTermListCommandSettings settings)
    {
        const SupportedLanguage sourceLanguage = SupportedLanguage.Spanish;
        const SupportedLanguage targetLanguage = SupportedLanguage.Polish;

        logger.LogInformation("Extracting terms from {InputFilePath}", settings.InputFilePath);
        var notes = await genericSpanishTermExtractor.ExtractTerms(settings.InputFilePath); // tech debt - extracts Notes and not just terms

        logger.LogInformation("Adding image candidates to notes...");
        var notesWithImages = await imageProvider.AddImageCandidates(notes, ImageGenerationProfile.PrivateDeck);

        logger.LogInformation("Adding audio to notes...");
        var notesWithImagesAndAudio = await audioProvider.AddAudio(notesWithImages, sourceLanguage, targetLanguage);

        logger.LogInformation("Performing Quality Assurance...");
        var qaChecked = await qualityControlService.AddQualitySuggestions(notesWithImagesAndAudio);

        var deck = new Deck("My Spanish lessons", qaChecked, "taurit", sourceLanguage, targetLanguage);
        await deckExporter.ExportToFolderAndOpenPreview(deck);

        return 0;
    }


}
