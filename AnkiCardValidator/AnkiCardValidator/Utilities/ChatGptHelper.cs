using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace AnkiCardValidator.Utilities;

public static class ChatGptHelper
{
    public static async Task<string> GetAnswerToPromptUsingChatGptApi(string systemChatMessage, string prompt, int attempt, bool jsonMode)
    {
        var appSettings = new Settings();
        var openAiClientOptions = new OpenAIClientOptions() { OrganizationId = appSettings.OpenAiOrganization };
        ChatClient client = new(model: Settings.OpenAiModelId, new ApiKeyCredential(appSettings.OpenAiDeveloperKey), openAiClientOptions);

        var stableHashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(prompt));
        var stableHash = BitConverter.ToString(stableHashBytes).Replace("-", string.Empty);

        var attemptNumberFileNameSuffix = attempt > 1 ? $"_attempt{attempt}" : string.Empty;
        var responseCacheFileName = $"{Settings.OpenAiModelId}_{stableHash}{attemptNumberFileNameSuffix}.txt";
        var responseToPromptFileName = Path.Combine(Settings.GptResponseCacheDirectory, responseCacheFileName);

        if (File.Exists(responseToPromptFileName))
        {
            Debug.WriteLine("Cached response from ChatGPT is used.");
            return responseToPromptFileName;
        }
        Debug.WriteLine($"Cache miss, querying ChatGPT API ({Settings.OpenAiModelId})...");

        ChatCompletionOptions options = new ChatCompletionOptions()
        {
            ResponseFormat = jsonMode ? ChatResponseFormat.JsonObject : ChatResponseFormat.Text

        };
        List<ChatMessage> messages =
        [
            new SystemChatMessage(systemChatMessage),
            new UserChatMessage(prompt)
        ];

        ChatCompletion completion = await client.CompleteChatAsync(messages, options);
        var responseToPrompt = completion.Content[0].Text;

        await File.WriteAllTextAsync(responseToPromptFileName, responseToPrompt);

        Debug.WriteLine($"Query finished, total tokens: input={completion.Usage.InputTokens}, output={completion.Usage.OutputTokens}");

        return responseToPromptFileName;
    }
}
