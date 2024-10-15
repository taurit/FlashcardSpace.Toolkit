using Azure.AI.OpenAI;
using CoreLibrary.Models;
using CoreLibrary.Utilities;
using Microsoft.Extensions.Logging;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;

namespace CoreLibrary.Services.GenerativeAiClients;

public class ChatGptClient(ILogger logger, OpenAiCredentials openAiCredentials, string persistentCacheRootFolder) : IGenerativeAiClient
{
    public async Task<string> GetAnswerToPrompt(string modelId, string modelClassId, string systemChatMessage, string prompt,
        GenerativeAiClientResponseMode mode, long seed, string? outputSchema = null)
    {
        return await GetAnswerToPrompt(modelId, modelClassId, systemChatMessage, prompt, mode, seed, outputSchema, useOpenAiBackendEvenIfAzureIsAvailable: false);
    }

    private async Task<string> GetAnswerToPrompt(string modelId, string modelClassId, string systemChatMessage, string prompt,
        GenerativeAiClientResponseMode mode, long seed, string? outputSchema, bool useOpenAiBackendEvenIfAzureIsAvailable)
    {
        if (mode is GenerativeAiClientResponseMode.StructuredOutput && outputSchema is null)
            throw new ArgumentException("Schema is required for StructuredOutput mode.");
        if (mode is not GenerativeAiClientResponseMode.StructuredOutput && outputSchema is not null)
            throw new ArgumentException("Schema is only allowed for StructuredOutput mode.");

        float? temperature = 0.1f; // default is 1; 0.1 is more deterministic and might be better for reliable language analysis with less creativity and randomness

        persistentCacheRootFolder.EnsureDirectoryExists();

        ChatClient? client = null;
        if (openAiCredentials.BackendType == OpenAiBackend.Azure && !useOpenAiBackendEvenIfAzureIsAvailable)
        {
            var azureOpenAiEndpointUrl = new Uri(openAiCredentials.AzureOpenAiEndpoint!);
            var azureOpenAiClient = new AzureOpenAIClient(
                azureOpenAiEndpointUrl,
                new ApiKeyCredential(openAiCredentials.AzureOpenAiKey!),
                new AzureOpenAIClientOptions()
                {
                    // I sometimes got timeouts after 1m 40 seconds, so here's an attempt to slightly increase it
                    NetworkTimeout = TimeSpan.FromMinutes(3)
                }
                );
            client = azureOpenAiClient.GetChatClient(modelId);
        }
        else
        {
            if (openAiCredentials.OpenAiDeveloperKey is null)
                throw new InvalidOperationException("OpenAI developer key is required to contact OpenAI backend.");

            var openAiClientOptions = new OpenAIClientOptions { OrganizationId = openAiCredentials.OpenAiOrganizationId };
            var openAiApiKeyCredential = new ApiKeyCredential(openAiCredentials.OpenAiDeveloperKey!);
            client = new(model: modelId, openAiApiKeyCredential, openAiClientOptions);
        }

        var stableHash = (prompt + outputSchema).GetHashCodeStable();
        // proper extensions helps debugging (e.g. VS Code highlights JSON files if they have proper extension only)
        var cacheFileExtension = mode == GenerativeAiClientResponseMode.PlainText ? "txt" : "json";

        var temperaturePart = ""; // todo hack to not invalidate cache on my pc. Remove at some point.
        if (temperature.HasValue)
            temperaturePart = $"_t{temperature.Value.ToString("0.0").Replace(",", ".")}";

        var responseCacheFileName = $"{modelClassId}_{stableHash}_r{seed}{temperaturePart}.{cacheFileExtension}";
        var responseToPromptFileName = Path.Combine(persistentCacheRootFolder, responseCacheFileName);

        if (File.Exists(responseToPromptFileName))
        {
            logger.LogDebug("Response for the prompt found in cache, re-using.");
            var fileContent = await File.ReadAllTextAsync(responseToPromptFileName);
            return fileContent;
        }
        logger.LogDebug($"Response not found in cache, retrieving response from model {modelId} (class: {modelClassId})...");

        var options = new ChatCompletionOptions();
        options.Seed = seed; // let's try to have some determinism in the responses to reduce test flakiness (beta feature)
        // comment: adds some determinism, but every few attempts I get different response with different system_fingerprint. So it's not reliable yet.
        if (temperature.HasValue)
            options.Temperature = temperature.Value;

        switch (mode)
        {
            case GenerativeAiClientResponseMode.PlainText:
                options.ResponseFormat = ChatResponseFormat.CreateTextFormat();
                break;
            case GenerativeAiClientResponseMode.JsonMode:
                options.ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat();
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
        if (completion.FinishReason == ChatFinishReason.ContentFilter)
        {
            logger.LogWarning("Azure OpenAI Content filter triggered, falling back to OpenAI API which doesn't have it and might work.");
            return await GetAnswerToPrompt(modelId, modelClassId, systemChatMessage, prompt, mode, seed, outputSchema, useOpenAiBackendEvenIfAzureIsAvailable: true);
        }
        if (completion.FinishReason != ChatFinishReason.Stop)
            throw new InvalidOperationException($"Chat completion finished with unexpected reason: {completion.FinishReason}");

        var responseToPrompt = completion.Content[0].Text;

        await File.WriteAllTextAsync(responseToPromptFileName, responseToPrompt);
        logger.LogDebug($"Query finished, the usage was {completion.Usage.InputTokenCount} input tokens and {completion.Usage.OutputTokenCount} output tokens.");

        return responseToPrompt;
    }
}
