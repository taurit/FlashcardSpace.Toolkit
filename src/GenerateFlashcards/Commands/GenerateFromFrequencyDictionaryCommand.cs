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
        DeckExporter deckExporter
    ) : AsyncCommand<GenerateFromFrequencyDictionarySettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, GenerateFromFrequencyDictionarySettings settings)
    {
        var terms = await frequencyDictionaryTermExtractor.ExtractTerms(
            settings.InputFilePath,
            settings.InputLanguage,
            settings.PartOfSpeechFilter,
            0,
            300);

        // shortcut: I assume terms are adjectives, todo: generalize
        var concreteAdjectives = await adjectivesSelector.SelectConcreteAdjectives(terms);

        logger.LogInformation("Selected EasyWords {PartOfSpeech}:", settings.PartOfSpeechFilter);
        logger.LogInformation("{@Terms}", concreteAdjectives.Select(x => x.TermOriginal));

        // export and open preview
        ExportToFolderAndOpenPreview(concreteAdjectives);

        return 0;
    }

    private void ExportToFolderAndOpenPreview(List<TermInContext> concreteAdjectives)
    {
        var deck = new Deck
        {
            Flashcards = concreteAdjectives.Select(x => new FlashcardNote()
            {
                Term = x.TermOriginal,
                Context = x.Sentence,
                Type = x.PartOfSpeech,
            }).ToList()
        };

        var tempSubfolderName = $"DeckExporter-{Guid.NewGuid().ToString().GetHashCodeStable(5)}";
        var singleUseExportDirectory = Path.Combine(Path.GetTempPath(), tempSubfolderName);
        Directory.CreateDirectory(singleUseExportDirectory);
        deckExporter.ExportDeck(deck, singleUseExportDirectory);
        deckExporter.OpenPreview(singleUseExportDirectory);
    }
}
