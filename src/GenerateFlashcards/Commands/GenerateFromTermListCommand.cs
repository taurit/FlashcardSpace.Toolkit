using GenerateFlashcards.Models;
using GenerateFlashcards.Services;
using Spectre.Console.Cli;

namespace GenerateFlashcards.Commands;

internal sealed class GenerateFromTermListCommand(
        GenericSpanishTermExtractor genericSpanishTermExtractor,
        DeckExporter deckExporter,
        SpanishToEnglishTranslationProvider spanishToEnglishTranslationProvider,
        SpanishToPolishTranslationProvider spanishToPolishTranslationProvider,
        ImageProvider imageProvider,
        AudioProvider audioProvider
    ) : AsyncCommand<GenerateFromTermListCommandSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, GenerateFromTermListCommandSettings settings)
    {
        const SupportedLanguage sourceLanguage = SupportedLanguage.Spanish;
        const SupportedLanguage targetLanguage = SupportedLanguage.Polish;

        // tech debt - extracts Notes and not just terms
        var notes = await genericSpanishTermExtractor.ExtractTerms(settings.InputFilePath);
        var notesWithImages = await imageProvider.AddImageCandidates(notes);
        var notesWithImagesAndAudio = await audioProvider.AddAudio(notesWithImages, sourceLanguage, targetLanguage);

        var deck = new Deck("My Spanish lessons", notesWithImagesAndAudio);
        deckExporter.ExportToFolderAndOpenPreview(deck);

        return 0;
    }


}
