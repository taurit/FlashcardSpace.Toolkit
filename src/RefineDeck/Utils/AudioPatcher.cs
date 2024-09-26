using CoreLibrary.Models;
using CoreLibrary.Services;
using CoreLibrary.Services.GenerativeAiClients.TextToSpeech;
using CoreLibrary.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.IO;

namespace RefineDeck.Utils;
internal static class AudioPatcher
{
    internal static AudioProvider GetAudioProviderInstance(DeckPath deckPath)
    {
        var audioCacheFolder = deckPath.AudioProviderCacheFolder;
        var audioProviderSettings = new AudioProviderSettings(audioCacheFolder);

        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<MainWindow>()
            .AddJsonFile("secrets.json", optional: true)
            .Build();

        // Bind the configuration values to the strongly typed class
        var secrets = new SecretParameters();
        configuration.Bind(secrets);

        var textToSpeechClient = new TextToSpeechClient(
            secrets.AZURE_TEXT_TO_SPEECH_KEY,
            secrets.AZURE_TEXT_TO_SPEECH_REGION,
            deckPath.TtsCacheFolder
        );
        var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<AudioProvider>();
        var audioProvider = new AudioProvider(audioProviderSettings, textToSpeechClient, logger);
        return audioProvider;
    }

    public static string ToRelativePath(string absolutePath, DeckPath deckPath)
    {
        // convert absolute path to relative path, relative to deckPath.DeckDataPath
        var relativePath = Path.GetRelativePath(deckPath.DeckDataPath, absolutePath);
        return relativePath;
    }
}
