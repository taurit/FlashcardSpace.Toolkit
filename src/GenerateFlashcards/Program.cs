using GenerateFlashcards.Commands;
using Spectre.Console.Cli;

var app = new CommandApp();

app.Configure(config =>
{
    config.AddCommand<GenerateFlashcardsCommand>("generate")
        .WithDescription("Generates language-learning flashcards from an input file.")
        .WithExample("generate", "--input", "input.txt")
        ;
});

await app.RunAsync(args);
