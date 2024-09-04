using GenerateFlashcards.Services;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;

namespace GenerateFlashcards.Commands;

internal sealed class GenerateFromFrequencyDictionaryCommand(
        ILogger<GenerateFromFrequencyDictionaryCommand> logger,
        FrequencyDictionaryTermExtractor frequencyDictionaryTermExtractor
    ) : AsyncCommand<GenerateFromWordListCommandSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, GenerateFromWordListCommandSettings settings)
    {

        return 0;
    }

}
