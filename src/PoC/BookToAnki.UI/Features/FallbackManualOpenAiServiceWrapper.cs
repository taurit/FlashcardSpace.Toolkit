using BookToAnki.UI.Components;
using BookToAnki.UI.OpenAiHumanInterface;
using System.Windows;

namespace BookToAnki.UI.Features;

public class FallbackManualOpenAiServiceWrapper : IFallbackManualOpenAiServiceWrapper
{
    public string CreateChatCompletion(string systemPrompt, string userPrompt)
    {
        var dialogWindow = new ChatGptHumanInterface($"{systemPrompt}\n\n{userPrompt}");

        dialogWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        dialogWindow.ShowDialog();

        var output = dialogWindow.DataContext as ChatGptHumanInterfaceViewModel;
        var response = output!.Response;
        return response;
    }
}
