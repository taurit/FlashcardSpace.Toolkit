using GenerateFlashcards.Commands;
using Spectre.Console.Cli;

namespace GenerateFlashcards;

internal class Program
{
    public static async Task<int> Main(string[] args)
    {
        var typeRegistrar = DependencyInjection.SetUpDependencyInjection();
        var app = new CommandApp(typeRegistrar);

        app.Configure(config =>
        {
#if DEBUG
            // convenient when running in Visual Studio to see the exception in place where it was thrown, and not just a message in the console output
            config.PropagateExceptions();
#endif
            config.AddCommand<GenerateFlashcardsCommand>("generate")
                .WithDescription("Generates language-learning flashcards from an input file.")
                .WithExample("generate", "--inputLanguage", "Spanish", "--outputLanguage", "English", "--inputFileFormat", "FrequencyDictionary", "input.txt")
                ;

            config.AddCommand<DebugCommand>("debug")
                .WithDescription("Runs a (temporary) debug command, running an arbitrary fragment of code I want to test in isolation.")
                .WithExample("debug")
                ;
        });


        var exitCode = await app.RunAsync(args);
        return exitCode;
    }
}
