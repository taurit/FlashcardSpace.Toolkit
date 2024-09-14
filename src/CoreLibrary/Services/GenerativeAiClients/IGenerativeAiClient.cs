namespace CoreLibrary.Services.GenerativeAiClients;

public interface IGenerativeAiClient
{
    /// <param name="modelId">OpenAI model id, e.g. `gpt-4o`</param>
    /// <param name="modelClassId">
    ///     Arbitrary model class identifier, used as cache key.
    ///     For example, of we want to consider cache outputs generated with model like `gpt-4o-preview`
    ///     still valid after we upgrade to `gpt-4o`, we just need to use the same <paramref name="modelClassId"/> value.
    /// </param>
    /// <param name="systemChatMessage"></param>
    /// <param name="prompt"></param>
    /// <param name="mode"></param>
    /// <param name="seed">Arbitrary seed number. Given the same seed, AI model should generate the same response.
    /// This reduces indeterminism (e.g. test flakiness) and makes debugging easier.</param>
    /// <param name="outputSchema"></param>
    Task<string> GetAnswerToPrompt(string modelId, string modelClassId,
        string systemChatMessage,
        string prompt,
        GenerativeAiClientResponseMode mode,
        long seed = 1,
        string? outputSchema = null);

}
