using BookToAnki.Services.OpenAi;
using System.IO;
using System.Windows;

namespace BookToAnki.UI.Components;

/// <summary>
/// Interaction logic for TextInputDialog.xaml
/// </summary>
public partial class SentenceTranslationDialog : Window
{
    // Services
    private readonly OpenAiServiceWrapper _openAi;
    private readonly string _wordOriginalForm;

    // Properties with UI bindings
    public string OriginalSentence { get; set; }
    public string UserProvidedSentenceTranslation { get; set; }
    public string? HumanTranslation { get; set; }
    public string? GoogleTranslation { get; set; }
    public string WiderContext { get; set; }
    public bool ScopeToWord { get; set; }

    public SentenceTranslationDialog(string question,
        string? defaultAnswer,
        string? humanTranslation,
        string? googleTranslation,
        string widerContext,
        string originalSentence,
        OpenAiServiceWrapper openAi,
        string wordOriginalForm)
    {
        _openAi = openAi;
        _wordOriginalForm = wordOriginalForm;
        InitializeComponent();
        DataContext = this;

        Title = question;

        UserProvidedSentenceTranslation = defaultAnswer ?? "";
        OriginalSentence = originalSentence;
        HumanTranslation = humanTranslation;
        GoogleTranslation = googleTranslation;
        WiderContext = widerContext;

        ExplainWithChatGPT4Mini_Click(null!, null!);
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }

    private static string SystemPrompt => File.ReadAllText("Resources/ExplainSentencePrompt.txt");
    public record Prompt(string SystemPrompt, string UserPrompt);
    public static Prompt PreparePrompt(string originalSentence, string wordOriginalForm)
    {
        return new Prompt(SystemPrompt, $"```{originalSentence}\n{wordOriginalForm}```");
    }

    private async void ExplainWithChatGPT4Mini_Click(object sender, RoutedEventArgs e)
    {
        var prompt = PreparePrompt(OriginalSentence, _wordOriginalForm);
        ChatGpt4oMiniExplanation.Text = await _openAi.CreateChatCompletion(prompt.SystemPrompt, prompt.UserPrompt, OpenAI.ObjectModels.Models.Gpt_4o_mini, false);
    }

    private async void ExplainWithChatGPT4o_Click(object sender, RoutedEventArgs e)
    {
        var prompt = PreparePrompt(OriginalSentence, _wordOriginalForm);
        ChatGpt4oExplanation.Text = await _openAi.CreateChatCompletion(prompt.SystemPrompt, prompt.UserPrompt, OpenAI.ObjectModels.Models.Gpt_4o, false);
    }

}
