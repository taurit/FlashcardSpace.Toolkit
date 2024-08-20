using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;
using System.Diagnostics.CodeAnalysis;

namespace GenerateFlashcards.Commands;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global", Justification = "Instantiated by Spectre.Console.Cli when needed")]
internal sealed class GenerateFlashcardsCommand(ILogger<GenerateFlashcardsCommand> logger) : AsyncCommand<GenerateFlashcardsCommandSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, GenerateFlashcardsCommandSettings settings)
    {
        logger.LogInformation("Generating flashcards");
        logger.LogInformation("Settings:\n\n{@Settings}\n", settings);


        return 0;
    }

}
