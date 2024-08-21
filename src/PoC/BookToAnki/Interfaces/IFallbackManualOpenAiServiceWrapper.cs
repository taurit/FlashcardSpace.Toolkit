namespace BookToAnki.UI.OpenAiHumanInterface;

public interface IFallbackManualOpenAiServiceWrapper
{
    string CreateChatCompletion(string systemPrompt, string userPrompt);
}
