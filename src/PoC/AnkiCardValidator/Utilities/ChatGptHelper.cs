using CoreLibrary.Models;
using CoreLibrary.Services.GenerativeAiClients;
using Microsoft.Extensions.Logging;

namespace AnkiCardValidator.Utilities;

public static class ChatGptHelper
{
    public static async Task<string> GetAnswerToPromptUsingChatGptApi(string systemChatMessage, string prompt, GenerativeAiClientResponseMode mode, int seed)
    {
        // adapt to use the refined service from CoreLibrary 
        var appSettings = new Settings();
        var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<ChatGptClient>();

        var openAiCredentials = new OpenAiCredentials(null, null, appSettings.OpenAiOrganizationId, appSettings.OpenAiDeveloperKey);

        var chatGptClient = new ChatGptClient(logger, openAiCredentials, Settings.GptResponseCacheDirectory);

        return await chatGptClient.GetAnswerToPrompt(Settings.OpenAiModelId, Settings.OpenAiModelGenerationId, systemChatMessage, prompt, mode, seed);
    }
}
