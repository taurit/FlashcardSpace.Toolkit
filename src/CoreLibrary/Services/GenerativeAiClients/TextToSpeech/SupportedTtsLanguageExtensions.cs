using CoreLibrary.Interfaces;

namespace CoreLibrary.Services.GenerativeAiClients.TextToSpeech;

public static class SupportedTtsLanguageExtensions
{
    public static SupportedTtsLanguage ToTtsLanguage(this SupportedInputLanguage inputLanguage)
    {
        return inputLanguage switch
        {
            SupportedInputLanguage.Spanish => SupportedTtsLanguage.Spanish,
            SupportedInputLanguage.English => SupportedTtsLanguage.English,
            _ => throw new NotImplementedException()
        };
    }

    public static SupportedTtsLanguage ToTtsLanguage(this SupportedOutputLanguage outputLanguage)
    {
        return outputLanguage switch
        {
            SupportedOutputLanguage.Spanish => SupportedTtsLanguage.Spanish,
            SupportedOutputLanguage.English => SupportedTtsLanguage.English,
            _ => throw new NotImplementedException()
        };
    }
}
