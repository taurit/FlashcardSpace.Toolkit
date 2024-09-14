namespace CoreLibrary.Services.GenerativeAiClients.TextToSpeech;
internal static class LanguageConfigurations
{
    // List of available voices: https://learn.microsoft.com/en-us/azure/ai-services/speech-service/language-support?tabs=tts
    internal static Dictionary<SupportedTtsLanguage, LanguageConfiguration> Configurations { get; } = new()
    {
        // I chose 1,20 by trial and error, in my opinion this sounds much more natural than 1.00
        { SupportedTtsLanguage.Spanish, new LanguageConfiguration("es-ES", "es-ES-TrianaNeural", "1.20") },
        { SupportedTtsLanguage.Polish, new LanguageConfiguration("pl-PL", "pl-PL-MarekNeural", "1.00") },
        { SupportedTtsLanguage.English, new LanguageConfiguration("en-US", "en-US-JacobNeural", "1.00") }
    };

}
