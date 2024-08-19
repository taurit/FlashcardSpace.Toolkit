using GenerateFlashcards.Commands;
using Spectre.Console.Cli;

internal class Program
{
    public static async Task<int> Main(string[] args)
    {
        var app = new CommandApp();

        app.Configure(config =>
        {
            config.AddCommand<GenerateFlashcardsCommand>("generate")
                .WithDescription("Generates language-learning flashcards from an input file.")
                .WithExample("generate", "--input", "input.txt")
                ;
        });

        var exitCode = await app.RunAsync(args);
        return exitCode;
    }
}
