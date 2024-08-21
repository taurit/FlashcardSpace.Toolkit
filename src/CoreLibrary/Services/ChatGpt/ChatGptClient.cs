using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace CoreLibrary.Services.ChatGpt;

public record PromptResponseFile(string FilePath)
{
    public async Task<string> GetContent() => await File.ReadAllTextAsync(FilePath);
}

public class ChatGptClient(string openAiOrganization, string openAiDeveloperKey, string persistentCacheRootFolder)
{
    /// <param name="modelId">OpenAI model id, e.g. `gpt-4o`</param>
    /// <param name="modelClassId">
    /// Arbitrary model class identifier, used as cache key.
    /// For example, of we want to consider cache outputs generated with model like `gpt-4o-preview`
    /// still valid after we upgrade to `gpt-4o`, we just need to use the same <paramref name="modelClassId"/> value.
    /// </param>
    public async Task<PromptResponseFile> FetchAnswerToPrompt(string modelId, string modelClassId, string systemChatMessage, string prompt, bool jsonMode)
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
            return new PromptResponseFile(responseToPromptFileName);
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

        return new PromptResponseFile(responseToPromptFileName);
    }
}
