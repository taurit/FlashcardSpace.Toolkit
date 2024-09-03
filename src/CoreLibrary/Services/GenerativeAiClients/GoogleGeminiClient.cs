using DotnetGeminiSDK.Client;
using DotnetGeminiSDK.Config;
using DotnetGeminiSDK.Model.Request;
using Microsoft.Extensions.Logging;

namespace CoreLibrary.Services.GenerativeAiClients;

public class GoogleGeminiClient(ILogger logger, string geminiApiKey, string persistentCacheRootFolder) : IGenerativeAiClient
{
    private string responseContent;

    /// <param name="modelId">Use model ID like:
    /// - gemini-pro (for the most recent `pro` class model)
    /// - gemini-1.5-pro (more specific)
    ///
    /// See also: https://ai.google.dev/pricing
    /// </param>
    public async Task<string> GetAnswerToPrompt(string modelId, string modelClassId, string systemChatMessage, string prompt, GenerativeAiClientResponseMode mode, string? outputSchema = null)
    {
        var config = new GoogleGeminiConfig
        {
            ApiKey = geminiApiKey,
            TextBaseUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-pro-latest"

        };
        var geminiClient = new GeminiClient(config);

        // model list:
        var response = await geminiClient.TextPrompt(prompt, new GenerationConfig()
        {
            MaxOutputTokens = 300,
            //ResponseMimeType = null //"application/json" // support for Structured Outputs not yet available in this version of library
        });


        var answer = response.Candidates.First();
        responseContent = answer.Content.Parts.First().Text;

        Console.WriteLine(responseContent);

        return responseContent;
    }
}
