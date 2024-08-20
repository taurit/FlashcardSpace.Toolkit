using GenerateFlashcards.Commands;
using GenerateFlashcards.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ReferenceWordExtractor;
using Spectre.Console.Cli;
using Vertical.SpectreLogger;
using Vertical.SpectreLogger.Options;

internal class Program
{
    public static async Task<int> Main(string[] args)
    {
        var typeRegistrar = SetUpDependencyInjection();
        var app = new CommandApp(typeRegistrar);

        app.Configure(config =>
        {
            config.AddCommand<GenerateFlashcardsCommand>("generate")
                .WithDescription("Generates language-learning flashcards from an input file.")
                .WithExample("generate", "--inputLanguage", "Spanish", "--outputLanguage", "English", "input.txt")
                ;
        });

        var exitCode = await app.RunAsync(args);
        return exitCode;
    }

    private static ServiceCollectionRegistrar SetUpDependencyInjection()
    {
        var services = new ServiceCollection();

        services.AddLogging(loggingBuilder =>
            loggingBuilder
                .AddSpectreConsole(spectreLoggingBuilder =>
                    spectreLoggingBuilder
                        .ConfigureProfiles(profile =>
                        {
                            profile.ConfigureOptions<DestructuringOptions>(ds => ds.WriteIndented = true);
                        })
                        // by default Trace and Debug are not logged; enable them
                        .SetMinimumLevel(LogLevel.Trace)
                )
                // also needed at this level to enable Trace and Debug
                .SetMinimumLevel(LogLevel.Trace)
        );
        services.AddSingleton<BuildingBlocksProvider>();
        services.AddSingleton<SimpleWordExtractor>();

        return new ServiceCollectionRegistrar(services);
    }
}
