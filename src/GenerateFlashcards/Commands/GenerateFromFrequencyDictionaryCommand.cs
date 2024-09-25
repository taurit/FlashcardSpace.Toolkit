using CoreLibrary.Models;
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
            200);

        // shortcut: I assume terms are adjectives, todo: generalize
        var concreteAdjectives = await adjectivesSelector.SelectConcreteAdjectives(terms);

        var notesWithEnglishTranslations = await spanishToEnglishTranslationProvider.AnnotateWithEnglishTranslation(concreteAdjectives);
        var notesWithEnglishAndPolishTranslations = await spanishToPolishTranslationProvider.AnnotateWithPolishTranslation(notesWithEnglishTranslations);
        var notesWithImages = await imageProvider.AddImageCandidates(notesWithEnglishAndPolishTranslations);
        var notesWithImagesAndAudio = await audioProvider.AddAudio(notesWithImages, settings.SourceLanguage, settings.OutputLanguage);

        var deck = new Deck("Spanish adjectives", notesWithImagesAndAudio, "fs-es-adj");
        deckExporter.ExportToFolderAndOpenPreview(deck);

        return 0;
    }


}
