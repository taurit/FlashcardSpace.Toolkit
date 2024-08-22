using Microsoft.Extensions.Logging;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using System.Security.Cryptography;
using System.Text;

namespace CoreLibrary.Services.GenerativeAiClients;

public class ChatGptClient(ILogger logger, string openAiOrganization, string openAiDeveloperKey, string persistentCacheRootFolder) : IGenerativeAiClient
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
            logger.LogDebug("Response for the prompt found in cache, re-using.");
            var fileContent = await File.ReadAllTextAsync(responseToPromptFileName);
            return fileContent;
        }
        logger.LogDebug($"Response not found in cache, retrieving response from model {modelId} (class: {modelClassId})...");

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
        logger.LogDebug($"Query finished, the usage was {completion.Usage.InputTokens} input tokens and {completion.Usage.OutputTokens} output tokens.");

        return responseToPrompt;
    }
}
