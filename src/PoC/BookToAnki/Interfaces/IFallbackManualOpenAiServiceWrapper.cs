namespace BookToAnki.Interfaces;

public interface IFallbackManualOpenAiServiceWrapper
{
    string CreateChatCompletion(string systemPrompt, string userPrompt);
}
