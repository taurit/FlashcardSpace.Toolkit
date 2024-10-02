using CoreLibrary.Models;
using CoreLibrary.Services;
using GenerateFlashcards.Services;
using Spectre.Console.Cli;

namespace GenerateFlashcards.Commands;

internal sealed class GenerateFromTermListCommand(
        GenericSpanishTermExtractor genericSpanishTermExtractor,
        DeckExporter deckExporter,
        ImageProvider imageProvider,
        AudioProvider audioProvider,
        QualityControlService qualityControlService
    ) : AsyncCommand<GenerateFromTermListCommandSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, GenerateFromTermListCommandSettings settings)
    {
        const SupportedLanguage sourceLanguage = SupportedLanguage.Spanish;
        const SupportedLanguage targetLanguage = SupportedLanguage.Polish;

        // tech debt - extracts Notes and not just terms
        var notes = await genericSpanishTermExtractor.ExtractTerms(settings.InputFilePath);
        var notesWithImages = await imageProvider.AddImageCandidates(notes, ImageGenerationProfile.PrivateDeck);
        var notesWithImagesAndAudio = await audioProvider.AddAudio(notesWithImages, sourceLanguage, targetLanguage);

        var qaChecked = await qualityControlService.AddQualitySuggestions(notesWithImagesAndAudio);

        var deck = new Deck("My Spanish lessons", qaChecked, "taurit", sourceLanguage, targetLanguage);
        await deckExporter.ExportToFolderAndOpenPreview(deck);

        return 0;
    }


}
