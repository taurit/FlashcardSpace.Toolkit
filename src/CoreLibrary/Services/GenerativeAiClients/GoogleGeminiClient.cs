using CoreLibrary.Utilities;
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
    public async Task<string> GetAnswerToPrompt(string modelId, string modelClassId, string systemChatMessage, string prompt, GenerativeAiClientResponseMode mode,
                                                long seed = 1, string? outputSchema = null)
    {
        var config = new GoogleGeminiConfig
        {
            ApiKey = geminiApiKey,
            TextBaseUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{modelId}",
        };
        var generationConfig = new GenerationConfig()
        {
            // Seed = seed, // already possible in API but not in SDK, I created an issue here: https://github.com/gsilvamartin/dotnet-gemini-sdk/issues/28
            MaxOutputTokens = 300,
            //ResponseMimeType = null //"application/json" // support for Structured Outputs not yet available in this version of library

            // Each model might have different range of temperature
            // https://cloud.google.com/vertex-ai/generative-ai/docs/learn/prompts/adjust-parameter-values#temperature
            Temperature = 0.1f,
            TopK = 2,

        };

        // do we have response cached already?
        var cacheFileName = GenerateCacheFileName(config.TextBaseUrl, generationConfig.MaxOutputTokens, prompt, seed);
        if (File.Exists(cacheFileName))
        {
            responseContent = await File.ReadAllTextAsync(cacheFileName);
            return responseContent;
        }

        var geminiClient = new GeminiClient(config);
        try
        {
            var response = await geminiClient.TextPrompt(prompt, generationConfig);
            var answer = response.Candidates.First();

            // Content filters are sometimes triggered with benign prompts like "Translate Spanish word `negro`".
            // Setting options like `HARM_CATEGORY_HATE_SPEECH=BLOCK_NONE` doesn't change the response in gemini-1.5-pro-002 (tested).
            // This code handles such cases by just mocking the response for now (another alternative is to fall back to OpenAI API, but it complicates the code,
            // and it's not clear if it's worth it)
            if (answer.FinishReason == "BLOCKLIST")
            {
                responseContent = "Response blocked by Gemini's content filter. Banned word in content? Review the flashcard manually.";
            }
            else
            {
                responseContent = answer.Content.Parts.First().Text;
            }
            // save back to cache
            await File.WriteAllTextAsync(cacheFileName, responseContent);

        }
        catch (Exception e) when (e.InnerException is not null && e.InnerException.Message.Contains("Please try again later."))
        {
            // Sometimes we receive:
            //{
            //    "error": {
            //        "code": 503,
            //        "message": "The model is overloaded. Please try again later.",
            //        "status": "UNAVAILABLE"
            //    }
            //}

            logger.LogWarning("Received error from Gemini API: {Error}", e.InnerException.Message);
            var timeout = TimeSpan.FromMinutes(1);
            logger.LogInformation($"Waiting for {timeout} before retrying...");
            await Task.Delay(timeout);
            return await GetAnswerToPrompt(modelId, modelClassId, systemChatMessage, prompt, mode, seed, outputSchema);
        }

        return responseContent;
    }

    private string GenerateCacheFileName(string configTextBaseUrl, int generationConfigMaxOutputTokens, string prompt, long seed)
    {
        var fingerprintPlainText = $"{configTextBaseUrl}_{generationConfigMaxOutputTokens}_{prompt}_{seed}";
        var fingerprint = fingerprintPlainText.GetHashCodeStable(10);
        var fileName = $"{fingerprint}.md";
        persistentCacheRootFolder.EnsureDirectoryExists();
        return Path.Combine(persistentCacheRootFolder, fileName);
    }
}
