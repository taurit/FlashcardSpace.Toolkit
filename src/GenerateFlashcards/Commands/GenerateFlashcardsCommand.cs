using Spectre.Console;
using Spectre.Console.Cli;
using System.Diagnostics.CodeAnalysis;

namespace GenerateFlashcards.Commands;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global", Justification = "Instantiated by Spectre.Console.Cli when needed")]
internal sealed class GenerateFlashcardsCommand : AsyncCommand<GenerateFlashcardsCommandSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, GenerateFlashcardsCommandSettings settings)
    {
        AnsiConsole.MarkupLine($"[underline bold]Generating flashcards[/]");
        AnsiConsole.WriteLine();

        AnsiConsole.MarkupLine($"[bold]Input file:[/] [aqua]{settings.InputFilePath}[/] (in [bold]{settings.InputLanguage}[/])");
        AnsiConsole.WriteLine();

        var table = new Table().Centered();

        // Synchronous
        await AnsiConsole.Progress()
            .StartAsync(async ctx =>
            {
                // Define tasks
                var task1 = ctx.AddTask("Extracting words from input");
                var task2 = ctx.AddTask("Determining parts of speech");
                var task3 = ctx.AddTask($"Translating words into [bold]{settings.OutputLanguage}[/]");
                var task4 = ctx.AddTask($"Generating audio files");
                var task5 = ctx.AddTask($"Generating picture candidates");

                while (!ctx.IsFinished)
                {
                    // Simulate some work
                    await Task.Delay(20);

                    // Increment
                    task1.Increment(1.5);
                    task2.Increment(0.5);

                    if (task2.IsFinished)
                    {
                        task3.Increment(100);
                        task4.Increment(100);
                        task5.Increment(100);
                    }
                }
            });

        AnsiConsole.MarkupLine("Launching the validation tool for human review...");
        // not implemented yet

        AnsiConsole.MarkupLine("Generating output files...");

        return 0;
    }

}
