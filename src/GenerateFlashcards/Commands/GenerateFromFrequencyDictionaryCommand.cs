using CoreLibrary.Models;
using CoreLibrary.Services.GenerativeAiClients.StableDiffusion;
using CoreLibrary.Services.GenerativeAiClients.TextToSpeech;
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
        AudioProvider audioProvider,
        QualityControlService qualityControlService
    ) : AsyncCommand<GenerateFromFrequencyDictionarySettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, GenerateFromFrequencyDictionarySettings settings)
    {
        var terms = await frequencyDictionaryTermExtractor.ExtractTerms(
            settings.InputFilePath,
            settings.SourceLanguage,
            settings.PartOfSpeechFilter,
            0,
            10000
            );

        // shortcut: I assume terms are adjectives, todo: generalize
        //var concreteAdjectives = await adjectivesSelector.SelectConcreteAdjectives(terms);

        var notesWithEnglishTranslations = await spanishToEnglishTranslationProvider.AnnotateWithEnglishTranslation(terms);
        var notesWithEnglishAndPolishTranslations = await spanishToPolishTranslationProvider.AnnotateWithPolishTranslation(notesWithEnglishTranslations);

        // quick hack to generate "Spanish Colors" deck without invalidating cache of previously generated images
        // todo this shouldn't be controlled via static property, requires a refactor
        StableDiffusionPromptProvider.AvoidInterferingWithColors = settings.InputFilePath.Contains("color", StringComparison.InvariantCultureIgnoreCase);

        var notesWithImages = await imageProvider.AddImageCandidates(notesWithEnglishAndPolishTranslations);
        var notesWithImagesAndAudio = await audioProvider.AddAudio(notesWithImages, settings.SourceLanguage, settings.OutputLanguage);

        var qaChecked = await qualityControlService.AddQualitySuggestions(notesWithImagesAndAudio);

        var deck = new Deck(settings.DeckName, qaChecked, settings.MediaFilesPrefix, settings.SourceLanguage, settings.OutputLanguage);
        await deckExporter.ExportToFolderAndOpenPreview(deck);

        return 0;
    }


}
