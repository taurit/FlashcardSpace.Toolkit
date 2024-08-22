using CoreLibrary.Services.GenerativeAiClients;
using Microsoft.Extensions.Logging;

namespace AnkiCardValidator.Utilities;

public static class ChatGptHelper
{
    public static async Task<string> GetAnswerToPromptUsingChatGptApi(string systemChatMessage, string prompt, bool jsonMode)
    {
        // adapt to use the refined service from CoreLibrary 
        var appSettings = new Settings();
        var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<ChatGptClient>();
        var chatGptClient = new ChatGptClient(logger, appSettings.OpenAiOrganizationId, appSettings.OpenAiDeveloperKey, Settings.GptResponseCacheDirectory);

        return await chatGptClient.GetAnswerToPrompt(Settings.OpenAiModelId, Settings.OpenAiModelGenerationId, systemChatMessage, prompt, jsonMode);
    }
}
