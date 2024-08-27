namespace CoreLibrary.Services.GenerativeAiClients;

/// <summary>
/// A mock implementation of the <see cref="IGenerativeAiClient"/> interface that can be used for testing purposes.
///
/// Generative AI is a key part of this application, but I don't want to make real API calls during automated tests
/// because of the cost and other factors. This mock implementation can be used instead to provide clearly fake responses
/// to prompts.
/// </summary>
public class MockGenerativeAiClient : IGenerativeAiClient
{
    public Task<string> GetAnswerToPrompt(string modelId, string modelClassId, string systemChatMessage, string prompt, GenerativeAiClientResponseMode mode,
        string? outputSchema = null)
    {
        return Task.FromResult("MOCK RESPONSE - API KEY NOT FOUND IN CONFIGURATION");
    }
}
