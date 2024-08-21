using BookToAnki.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using NeoSmart.Caching.Sqlite;
using OpenAI;
using OpenAI.Managers;
using OpenAI.ObjectModels.RequestModels;
using System.Diagnostics;

namespace BookToAnki.Services.OpenAi;

public class OpenAiServiceWrapper
{
    private readonly IFallbackManualOpenAiServiceWrapper _fallbackService;
    private readonly OpenAIService _openAiService;

    private readonly SqliteCache _cache;
    private readonly DistributedCacheEntryOptions _noExpirationPolicy = new();

    public decimal TotalCostUsd { get; private set; }

    public OpenAiServiceWrapper(string developerKey, string organization, string cacheFile, IFallbackManualOpenAiServiceWrapper fallbackService)
    {
        _fallbackService = fallbackService;
        _openAiService = new OpenAIService(new OpenAiOptions
        {
            ApiKey = developerKey,
            Organization = organization
        }, new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(360)
        });

        _cache = new SqliteCache(new SqliteCacheOptions
        {
            CachePath = cacheFile
        });
    }

    /// <param name="model">E.g. OpenAI.ObjectModels.Models.Gpt_3_5_Turbo_1106</param>
    /// <param name="jsonMode">Enables JSON mode in the API. Be careful, because JSON mode doesn't know how to return arrays and will fail if the prompt asks for array, and not an object.</param>
    public async Task<string> CreateChatCompletion(string systemPrompt, string userPrompt, string model, bool jsonMode)
    {
        // Try retrieve response from cache
        var cacheKey = $"{model}{(jsonMode ? "#json" : "")}#{systemPrompt}#{userPrompt}";
        var response = await _cache.GetStringAsync(cacheKey);
        if (!String.IsNullOrWhiteSpace(response)) return response;

        // Not found in cache.
        string? responseContent = null;
        if (model == OpenAI.ObjectModels.Models.Gpt_4o_mini || model == OpenAI.ObjectModels.Models.Gpt_4o)
        {
            try
            {
                var completionResult = await _openAiService.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest
                {
                    Messages = new List<ChatMessage>
                    {
                        ChatMessage.FromSystem(systemPrompt),
                        ChatMessage.FromUser(userPrompt)
                    },
                    Model = model,
                    MaxTokens = 1000, // optional; $0.002 / 1K tokens for gpt-3.5-turbo; $0.03 for GPT4
                    ResponseFormat = new ResponseFormat
                    {
                        Type = jsonMode
                            ? OpenAI.ObjectModels.StaticValues.CompletionStatics.ResponseFormat.Json
                            : OpenAI.ObjectModels.StaticValues.CompletionStatics.ResponseFormat.Text
                    }
                });

                bool isGpt4o = model == OpenAI.ObjectModels.Models.Gpt_4o;
                var pricePerPromptTokenUsd = isGpt4o ? 5m / 1_000_000 : 0.15m / 1_000_000;
                var pricePerCompletionTokenUsd = isGpt4o ? 15m / 1_000_000 : 0.6m / 1_000_000;

                if (completionResult.Successful)
                {
                    Debug.WriteLine($"Prompt tokens: {completionResult.Usage.PromptTokens}, Completion tokens: {completionResult.Usage.CompletionTokens}");

                    // https://help.openai.com/en/articles/7127956-how-much-does-gpt-4-cost
                    var promptCostUsd = completionResult.Usage.PromptTokens * pricePerPromptTokenUsd;
                    var responseCostUsd = (completionResult.Usage.CompletionTokens ?? 0) * pricePerCompletionTokenUsd;
                    decimal costUsd = promptCostUsd + responseCostUsd;
                    TotalCostUsd += costUsd;
                    var firstCompletion = completionResult.Choices.First();
                    responseContent = firstCompletion.Message.Content;
                }
                else
                {
                    throw new InvalidOperationException($"{completionResult.Error?.Message} ({completionResult.Error?.Code}, {completionResult.Error?.Type}, {completionResult.Error?.Param})");
                }
            }
            catch (TaskCanceledException)
            {
                // System.Threading.Tasks.TaskCanceledException: 'The request was canceled due to the configured HttpClient.Timeout of 360 seconds elapsing.'
                // SocketException: The I/O operation has been aborted because of either a thread exit or an application request.
                return "Task canceled, most likely no one is waiting for this response.";
            }
        }
        else
        {
            // Fall back to manual query? Not sure what model could that be
            responseContent = _fallbackService.CreateChatCompletion(systemPrompt, userPrompt);
        }

        await _cache.SetStringAsync(cacheKey, responseContent, _noExpirationPolicy);

        return responseContent;
    }

}

