using CoreLibrary.Utilities;
using Microsoft.Extensions.Logging;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;

namespace CoreLibrary.Services.GenerativeAiClients;

public class ChatGptClient : IGenerativeAiClient
{
    private readonly ILogger _logger;
    private readonly string _openAiOrganization;
    private readonly string _openAiDeveloperKey;
    private readonly string _persistentCacheRootFolder;

    public ChatGptClient(ILogger logger, string openAiOrganization, string openAiDeveloperKey, string persistentCacheRootFolder)
    {
        _logger = logger;
        _openAiOrganization = openAiOrganization;
        _openAiDeveloperKey = openAiDeveloperKey;
        _persistentCacheRootFolder = persistentCacheRootFolder;

        // Ensure the folder exists (does nothing if it exists already
        Directory.CreateDirectory(persistentCacheRootFolder);
    }

    public async Task<string> GetAnswerToPrompt(string modelId, string modelClassId, string systemChatMessage, string prompt,
        GenerativeAiClientResponseMode mode, long seed, string? outputSchema = null)
    {
        if (mode is GenerativeAiClientResponseMode.StructuredOutput && outputSchema is null)
            throw new ArgumentException("Schema is required for StructuredOutput mode.");
        if (mode is not GenerativeAiClientResponseMode.StructuredOutput && outputSchema is not null)
            throw new ArgumentException("Schema is only allowed for StructuredOutput mode.");

        var openAiClientOptions = new OpenAIClientOptions { OrganizationId = _openAiOrganization };
        ChatClient client = new(model: modelId, new ApiKeyCredential(_openAiDeveloperKey), openAiClientOptions);

        var stableHash = (prompt + outputSchema).GetHashCodeStable();
        // proper extensions helps debugging (e.g. VS Code highlights JSON files if they have proper extension only)
        var cacheFileExtension = mode == GenerativeAiClientResponseMode.PlainText ? "txt" : "json";
        var responseCacheFileName = $"{modelClassId}_{stableHash}_r{seed}.{cacheFileExtension}";
        var responseToPromptFileName = Path.Combine(_persistentCacheRootFolder, responseCacheFileName);

        if (File.Exists(responseToPromptFileName))
        {
            _logger.LogDebug("Response for the prompt found in cache, re-using.");
            var fileContent = await File.ReadAllTextAsync(responseToPromptFileName);
            return fileContent;
        }
        _logger.LogDebug($"Response not found in cache, retrieving response from model {modelId} (class: {modelClassId})...");

        var options = new ChatCompletionOptions();
        options.Seed = seed; // let's try to have some determinism in the responses to reduce test flakiness

        switch (mode)
        {
            case GenerativeAiClientResponseMode.PlainText:
                options.ResponseFormat = ChatResponseFormat.Text;
                break;
            case GenerativeAiClientResponseMode.JsonMode:
                options.ResponseFormat = ChatResponseFormat.JsonObject;
                break;
            case GenerativeAiClientResponseMode.StructuredOutput:
                options.ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                    "transformed_input_schema",
                    BinaryData.FromString(outputSchema!),
                    "Output schema containing an array of output items, each of which corresponds to one input item and is linked to it by the Id property."
                    );
                break;
        }

        List<ChatMessage> messages =
        [
            new SystemChatMessage(systemChatMessage),
            new UserChatMessage(prompt)
        ];

        ChatCompletion completion = await client.CompleteChatAsync(messages, options);
        var responseToPrompt = completion.Content[0].Text;

        await File.WriteAllTextAsync(responseToPromptFileName, responseToPrompt);
        _logger.LogDebug($"Query finished, the usage was {completion.Usage.InputTokens} input tokens and {completion.Usage.OutputTokens} output tokens.");

        return responseToPrompt;
    }
}
