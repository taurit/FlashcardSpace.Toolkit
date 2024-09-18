using CoreLibrary.Interfaces;

namespace CoreLibrary.Services.GenerativeAiClients.TextToSpeech;
internal static class LanguageConfigurations
{
    // List of available voices: https://learn.microsoft.com/en-us/azure/ai-services/speech-service/language-support?tabs=tts
    internal static Dictionary<SupportedLanguage, LanguageConfiguration> Configurations { get; } = new()
    {
        // I chose 1,20 by trial and error, in my opinion this sounds much more natural than 1.00
        { SupportedLanguage.Spanish, new LanguageConfiguration("es-ES", "es-ES-TrianaNeural", "1.20") },
        { SupportedLanguage.Polish, new LanguageConfiguration("pl-PL", "pl-PL-MarekNeural", "1.00") },
        { SupportedLanguage.English, new LanguageConfiguration("en-US", "en-US-JacobNeural", "1.00") }
    };

}
