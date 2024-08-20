using DotnetGeminiSDK.Client;
using DotnetGeminiSDK.Config;

namespace AnkiCardValidator.Utilities;
public class GeminiHelper
{
    public async Task<string> GetAnswer(string prompt)
    {
        var settings = new Settings();
        var config = new GoogleGeminiConfig()
        {
            ApiKey = settings.GeminiApiKey,
            // default TextBaseUrl is for the "pro" model https://generativelanguage.googleapis.com/v1/models/gemini-pro

        };
        var geminiClient = new GeminiClient(config);
        var response = await geminiClient.TextPrompt(prompt);
        var answer = response.Candidates.First();
        return answer.Content.Parts.First().Text;
    }
}
