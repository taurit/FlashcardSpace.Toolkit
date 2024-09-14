using GenerateFlashcards.Services;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;

namespace GenerateFlashcards.Commands;

internal sealed class GenerateFromFrequencyDictionaryCommand(
        ILogger<GenerateFromFrequencyDictionaryCommand> logger,
        FrequencyDictionaryTermExtractor frequencyDictionaryTermExtractor
    ) : AsyncCommand<GenerateFromFrequencyDictionarySettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, GenerateFromFrequencyDictionarySettings settings)
    {
        var terms = await frequencyDictionaryTermExtractor.ExtractTerms(settings.InputFilePath,
            settings.InputLanguage, 0, 300);

        if (settings.PartOfSpeechFilter is not null)
        {
            terms = terms.Where(term => term.PartOfSpeech == settings.PartOfSpeechFilter).ToList();
        }

        logger.LogInformation("Terms filtered to {PartOfSpeech}:", settings.PartOfSpeechFilter);
        logger.LogInformation("{@Terms}", terms);

        return 0;
    }

}
