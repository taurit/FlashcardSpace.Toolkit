using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace AnkiCardValidator.Utilities;

internal static class ChatGptHelper
{
    internal static async Task<string> GetAnswerToPromptUsingChatGptApi(string prompt)
    {
        var appSettings = new Settings();
        var openAiClientOptions = new OpenAIClientOptions() { OrganizationId = appSettings.OpenAiOrganization };
        ChatClient client = new(model: Settings.OpenAiModelId, new ApiKeyCredential(appSettings.OpenAiDeveloperKey), openAiClientOptions);

        var stableHashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(prompt));
        var stableHash = BitConverter.ToString(stableHashBytes).Replace("-", string.Empty);

        var storyCacheFileName = $"{Settings.OpenAiModelId}_{stableHash}.txt";
        var responseToPromptFileName = Path.Combine(Settings.GptResponseCacheDirectory, storyCacheFileName);

        if (File.Exists(responseToPromptFileName))
        {
            Debug.WriteLine("Cached response from ChatGPT is used.");
            return responseToPromptFileName;
        }
        Debug.WriteLine("Cache miss, querying ChatGPT API...");

        ChatCompletionOptions options = new ChatCompletionOptions()
        {
            ResponseFormat = ChatResponseFormat.JsonObject
        };
        List<ChatMessage> messages = new List<ChatMessage>
            {
                new SystemChatMessage("You are an assistant to help students of Spanish language evaluate the quality of flashcards. Students already know Polish and English."),
                new UserChatMessage(prompt)
            };

        ChatCompletion completion = await client.CompleteChatAsync(messages, options);
        var responseToPrompt = completion.Content[0].Text;

        await File.WriteAllTextAsync(responseToPromptFileName, responseToPrompt);

        Debug.WriteLine($"Query finished, total tokens: input={completion.Usage.InputTokens}, output={completion.Usage.OutputTokens}");

        return responseToPromptFileName;
    }
}
