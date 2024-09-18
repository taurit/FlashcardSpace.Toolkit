using CoreLibrary.Utilities;
using GenerateFlashcards.Models;
using GenerateFlashcards.Services;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;

namespace GenerateFlashcards.Commands;

internal sealed class GenerateFromFrequencyDictionaryCommand(
        ILogger<GenerateFromFrequencyDictionaryCommand> logger,
        FrequencyDictionaryTermExtractor frequencyDictionaryTermExtractor,
        EasyWordsSpanishAdjectivesSelector adjectivesSelector,
        DeckExporter deckExporter,
        SpanishToEnglishTranslationProvider spanishToEnglishTranslationProvider,
        SpanishToPolishTranslationProvider spanishToPolishTranslationProvider,
        ImageProvider imageProvider,
        AudioProvider audioProvider
    ) : AsyncCommand<GenerateFromFrequencyDictionarySettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, GenerateFromFrequencyDictionarySettings settings)
    {
        var terms = await frequencyDictionaryTermExtractor.ExtractTerms(
            settings.InputFilePath,
            settings.SourceLanguage,
            settings.PartOfSpeechFilter,
            0,
            100);

        // shortcut: I assume terms are adjectives, todo: generalize
        var concreteAdjectives = await adjectivesSelector.SelectConcreteAdjectives(terms);

        var notesWithEnglishTranslations = await spanishToEnglishTranslationProvider.AnnotateWithEnglishTranslation(terms);
        var notesWithEnglishAndPolishTranslations = await spanishToPolishTranslationProvider.AnnotateWithPolishTranslation(notesWithEnglishTranslations);
        var notesWithImages = await imageProvider.AddImageCandidates(notesWithEnglishAndPolishTranslations);
        var notesWithImagesAndAudio = await audioProvider.AddAudio(notesWithImages, settings.SourceLanguage, settings.OutputLanguage);

        // export and open preview
        ExportToFolderAndOpenPreview(notesWithImagesAndAudio);

        return 0;
    }

    private void ExportToFolderAndOpenPreview(List<FlashcardNote> flashcards)
    {
        var deck = new Deck
        {
            Flashcards = flashcards.ToList()
        };

        var tempSubfolderName = $"DeckExporter-{Guid.NewGuid().ToString().GetHashCodeStable(5)}";
        var singleUseExportDirectory = Path.Combine(Path.GetTempPath(), tempSubfolderName);
        Directory.CreateDirectory(singleUseExportDirectory);
        deckExporter.ExportDeck(deck, singleUseExportDirectory);
        deckExporter.OpenPreview(singleUseExportDirectory);
    }
}
