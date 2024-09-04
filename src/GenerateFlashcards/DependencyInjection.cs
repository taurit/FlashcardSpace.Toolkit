using CoreLibrary.Services;
using CoreLibrary.Services.GenerativeAiClients;
using CoreLibrary.Services.ObjectGenerativeFill;
using GenerateFlashcards.Models;
using GenerateFlashcards.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Vertical.SpectreLogger;
using Vertical.SpectreLogger.Options;

namespace GenerateFlashcards;

internal static class DependencyInjection
{
    internal static ServiceCollectionRegistrar SetUpDependencyInjection()
    {
        var services = new ServiceCollection();

        // Add logging
        services.AddLogging(loggingBuilder =>
            loggingBuilder
                .AddSpectreConsole(spectreLoggingBuilder =>
                    spectreLoggingBuilder
                        .ConfigureProfiles(profile =>
                        {
                            profile.ConfigureOptions<DestructuringOptions>(ds => ds.WriteIndented = true);
                            //profile.OutputTemplate = "{Message}{NewLine}{Exception}";
                        })
                        // by default Trace and Debug are not logged; enable them
                        .SetMinimumLevel(LogLevel.Debug)
                )
                // also needed at this level to enable Trace and Debug
                .SetMinimumLevel(LogLevel.Debug)
        );

        // Get instance of ILogger which we need already at this point
        var serviceProvider = services.BuildServiceProvider();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        ILogger<Program> logger = loggerFactory.CreateLogger<Program>();

        // Read secrets
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .AddJsonFile("secrets.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        // Bind the configuration values to the strongly typed class
        var secretsConfiguration = new SecretsConfiguration();
        configuration.Bind(secretsConfiguration);
        var openAiApiKeysPresent = secretsConfiguration.EnsureOpenAIKeysArePresent(logger);
        services.AddSingleton(secretsConfiguration);

        // Add HttpClient (what Nuget package is needed?)
        services.AddHttpClient();

        services.AddSingleton<ImageGenerator>();
        services.AddSingleton<NormalFormProvider>();

        // Add other services
        services.AddTransient<ReferenceTranslator>();
        services.AddTransient<FrequencyDictionaryTermExtractor>();
        services.AddTransient<AdvancedSentenceExtractor>();

        IGenerativeAiClient generativeAiClient = openAiApiKeysPresent
            ? new ChatGptClient(
                logger,
                secretsConfiguration.OPENAI_ORGANIZATION_ID!,
                secretsConfiguration.OPENAI_DEVELOPER_KEY!,
                Parameters.ChatGptClientCacheFolder.Value
            )
            : new MockGenerativeAiClient();

        services.AddSingleton(generativeAiClient);

        GenerativeFill generativeFill = new(generativeAiClient, Parameters.GenerativeFillCacheFolder.Value);
        services.AddSingleton(generativeFill);

        return new ServiceCollectionRegistrar(services);
    }
}
