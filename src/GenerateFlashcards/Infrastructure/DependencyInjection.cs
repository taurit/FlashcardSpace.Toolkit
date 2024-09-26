using CoreLibrary.Services;
using CoreLibrary.Services.GenerativeAiClients;
using CoreLibrary.Services.GenerativeAiClients.TextToSpeech;
using CoreLibrary.Services.ObjectGenerativeFill;
using CoreLibrary.Utilities;
using GenerateFlashcards.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Vertical.SpectreLogger;
using Vertical.SpectreLogger.Options;

namespace GenerateFlashcards.Infrastructure;

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
                        .SetMinimumLevel(LogLevel.Information)
                )
                // also needed at this level to enable Trace and Debug
                .SetMinimumLevel(LogLevel.Information)
                .AddFilter("System.Net.Http.HttpClient", LogLevel.Warning)  // or LogLevel.None if you want to suppress all

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
        var secretParameters = new SecretParameters();
        configuration.Bind(secretParameters);
        var openAiApiKeysPresent = secretParameters.WarnIfGenerativeAIKeysAreNotPresent(logger);
        services.AddSingleton(secretParameters);

        // Add HttpClient (what Nuget package is needed?)
        // Add HttpClient (what Nuget package is needed?)
        services.AddHttpClient();

        var stableDiffusionPromptProvider = new StableDiffusionPromptProvider(logger);
        services.AddSingleton<StableDiffusionPromptProvider>(stableDiffusionPromptProvider);

        var imageGenerationSettings = new ImageGeneratorSettings(Parameters.ImageGeneratorCacheFolder);
        services.AddSingleton(imageGenerationSettings);
        services.AddSingleton<ImageGenerator>();

        var imageProviderSettings = new ImageCandidatesProviderSettings(Parameters.ImageProviderCacheFolder);
        services.AddSingleton(imageProviderSettings);

        var audioProviderSettings = new AudioProviderSettings(Parameters.AudioCacheFolder);
        services.AddSingleton(audioProviderSettings);

        services.AddSingleton<NormalFormProvider>();

        // Add other services
        services.AddTransient<FrequencyDictionaryTermExtractor>();
        services.AddTransient<GenericSpanishTermExtractor>();
        services.AddTransient<AdvancedSentenceExtractor>();
        services.AddTransient<EasyWordsSpanishAdjectivesSelector>();
        services.AddTransient<SpanishToEnglishTranslationProvider>();
        services.AddTransient<SpanishToPolishTranslationProvider>();
        services.AddTransient<QualityControlService>();
        services.AddTransient<ImageCandidatesGenerator>();
        services.AddTransient<ImageProvider>();
        services.AddTransient<AudioProvider>();

        IGenerativeAiClient generativeAiClient = openAiApiKeysPresent
            ? new ChatGptClient(
                logger,
                secretParameters.OPENAI_ORGANIZATION_ID!,
                secretParameters.OPENAI_DEVELOPER_KEY!,
                Parameters.ChatGptClientCacheFolder
            )
            : new MockGenerativeAiClient();

        services.AddSingleton(generativeAiClient);

        GenerativeFill generativeFill = new(generativeAiClient, Parameters.GenerativeFillCacheFolder);
        services.AddSingleton(generativeFill);

        TextToSpeechClient ttsClient = new TextToSpeechClient(
            secretParameters.AZURE_TEXT_TO_SPEECH_KEY!,
            secretParameters.AZURE_TEXT_TO_SPEECH_REGION!,
            Parameters.TextToSpeechCacheFolder
            );
        services.AddSingleton(ttsClient);

        DeckExporterSettings deckExporterSettings = new DeckExporterSettings(
            Parameters.BrowserProfileDirectory,
            Parameters.DeckExportPath
            );

        services.AddSingleton(deckExporterSettings);
        services.AddTransient<DeckExporter>();

        return new ServiceCollectionRegistrar(services);
    }
}


