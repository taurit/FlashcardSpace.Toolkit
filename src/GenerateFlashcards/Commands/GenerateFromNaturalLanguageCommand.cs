using GenerateFlashcards.Services;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;

namespace GenerateFlashcards.Commands;

internal sealed class GenerateFromNaturalLanguageCommand(
        ILogger<GenerateFromNaturalLanguageCommand> logger,
        AdvancedSentenceExtractor sentencesExtractor
    ) : AsyncCommand<GenerateFromNaturalLanguageSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, GenerateFromNaturalLanguageSettings settings)
    {
        // See Architecture.drawio -> "Pipeline design" for the general idea of the flow.
        var sentences = await sentencesExtractor.ExtractSentences(settings.InputFilePath);

        throw new NotImplementedException("The rest of the implementation is not yet done. " +
                                          "I'm prioritizing the flow generating flashcards from frequency dictionary, " +
                                          "not plain text.");
    }

}
