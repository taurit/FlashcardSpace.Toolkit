using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace CoreLibrary.Services.GenerativeAiClients;

public class ChatGptClient(string openAiOrganization, string openAiDeveloperKey, string persistentCacheRootFolder) : IGenerativeAiClient
{
    public async Task<string> GetAnswerToPrompt(string modelId, string modelClassId, string systemChatMessage, string prompt, bool jsonMode)
    {
        var openAiClientOptions = new OpenAIClientOptions { OrganizationId = openAiOrganization };
        ChatClient client = new(model: modelId, new ApiKeyCredential(openAiDeveloperKey), openAiClientOptions);

        var stableHashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(prompt));
        var stableHash = BitConverter.ToString(stableHashBytes).Replace("-", string.Empty);

        var responseCacheFileName = $"{modelClassId}_{stableHash}.txt";
        var responseToPromptFileName = Path.Combine(persistentCacheRootFolder, responseCacheFileName);

        if (File.Exists(responseToPromptFileName))
        {
            Debug.WriteLine("Cached response from ChatGPT is used.");
            var fileContent = await File.ReadAllTextAsync(responseToPromptFileName);
            return fileContent;
        }
        Debug.WriteLine($"Cache miss, querying ChatGPT API (model: {modelId}, model class: {modelClassId})...");

        var options = new ChatCompletionOptions
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

        return responseToPrompt;
    }
}
