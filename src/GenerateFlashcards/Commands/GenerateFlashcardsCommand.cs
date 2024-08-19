using Spectre.Console;
using Spectre.Console.Cli;
using System.Diagnostics.CodeAnalysis;

namespace GenerateFlashcards.Commands;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global", Justification = "Instantiated by Spectre.Console.Cli when needed")]
internal sealed class GenerateFlashcardsCommand : Command<GenerateFlashcardsCommandSettings>
{
    public override int Execute([NotNull] CommandContext context, [NotNull] GenerateFlashcardsCommandSettings settings)
    {
        AnsiConsole.MarkupLine($"Running with args like [green]{settings.InputFilePath}[/], {settings.InputLanguage}");

        return 0;
    }
}
