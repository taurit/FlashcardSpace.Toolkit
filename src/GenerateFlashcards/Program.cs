using GenerateFlashcards.Commands;
using GenerateFlashcards.Infrastructure;
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
            config.AddCommand<GenerateFromFrequencyDictionaryCommand>("generate-from-frequency-dictionary")
                .WithDescription("Generates flashcards with from an input containing word frequency dictionary.")
                .WithExample("generate-from-frequency-dictionary", "--inputLanguage", "Spanish", "--outputLanguage", "English", "input.txt")
                ;

            // experimental; source is a plain text with lines containing terms to learn; possibly with typos
            config.AddCommand<GenerateFromTermListCommand>("generate-from-term-list")
                .WithDescription("Generates flashcards from a list of Spanish words and phrases I want to learn.")
                .WithExample("generate-from-term-list", "words.txt")
                ;


            config.AddCommand<GenerateFromNaturalLanguageCommand>("generate-from-natural-language")
                .WithDescription("Generates flashcards from an input containing plain text in natural language.")
                .WithExample("generate-from-natural-language", "--inputLanguage", "Spanish", "--outputLanguage", "English", "input.txt")
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
