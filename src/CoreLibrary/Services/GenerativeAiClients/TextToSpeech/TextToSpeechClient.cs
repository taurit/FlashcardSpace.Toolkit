using CoreLibrary.Interfaces;
using CoreLibrary.Utilities;
using Microsoft.CognitiveServices.Speech;

namespace CoreLibrary.Services.GenerativeAiClients.TextToSpeech;

/// <summary>
/// Generates text-to-speech audio files for flashcards using Azure TTS service
/// </summary>
public class TextToSpeechClient(string speechKey, string speechRegion, string cacheFolder)
{
    public async Task<byte[]> GenerateAudioFile(string text, SupportedTtsLanguage language)
    {
        cacheFolder.EnsureDirectoryExists();

        var languageConfiguration = LanguageConfigurations.Configurations[language];
        var speechConfig = SpeechConfig.FromSubscription(speechKey, speechRegion);

        // MP3 format seems to guarantee the least compatibility problems; I went a bit high with the bit rate (96KBit)
        // so there's a room to reduce size if needed
        speechConfig.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Audio24Khz96KBitRateMonoMp3);

        var ssml = $@"
    <speak version='1.0' xml:lang='{languageConfiguration.LanguageCode}'>
        <voice name='{languageConfiguration.VoiceName}'>
            <prosody rate='{languageConfiguration.ProsodyRate}'>{text}</prosody>
        </voice>
    </speak>";

        var cacheFileName = GenerateCacheFileName(text, ssml);
        if (File.Exists(cacheFileName))
            return await File.ReadAllBytesAsync(cacheFileName);

        using var speechSynthesizer = new SpeechSynthesizer(speechConfig);
        var speechSynthesisResult = await speechSynthesizer.SpeakSsmlAsync(ssml);
        var audioData = speechSynthesisResult.AudioData;
        await File.WriteAllBytesAsync(cacheFileName, audioData);

        return audioData;
    }

    public Task<byte[]> GenerateAudioFile(string text, SupportedInputLanguage supportedLanguage) =>
        GenerateAudioFile(text, supportedLanguage.ToTtsLanguage());

    public Task<byte[]> GenerateAudioFile(string text, SupportedOutputLanguage supportedOutputLanguage) =>
        GenerateAudioFile(text, supportedOutputLanguage.ToTtsLanguage());

    private string GenerateCacheFileName(string text, string ssml)
    {
        var fileName =
            // text part is just for readability of file list and ease of debugging
            $"{text.ToFilenameFriendlyString()}_" +
            // ssml contains all unique characteristics of a request
            $"{ssml.GetHashCodeStable(7)}" +
            $".mp3";

        var path = Path.Combine(cacheFolder, fileName);
        return path;
    }
}
