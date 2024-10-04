using CoreLibrary.Services.GenerativeAiClients;
using CoreLibrary.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RefineDeck.ViewModels;

namespace RefineDeck.Utils;

internal class GeminiQualityAssuranceAgent(MainWindowViewModel viewModel)
{
    GoogleGeminiClient? _client = null;

    public async Task ValidateSelectedCard()
    {
        _client ??= GetGeminiClientInstance();

        var card = viewModel.SelectedFlashcard;
        if (card is null) return;

        var prompt = "Validate the correctness and quality of the flashcard provided below. If anything needs change for correctness or clarification, list brief and concrete suggestions for the flashcard author.\n" +
                     "\n" +
                     $"# Front side\n" +
                     $"Question in Spanish: {card.Term}\n" +
                     $"\n" +
                     $"# Back side\n" +
                     $"Answer in Polish: {card.TermTranslation}\n" +
                     $"Sentence example in Spanish: {card.SentenceExample}" +
                     $"Sentence example translation in Polish: {card.SentenceExampleTranslation}" +
                     $"Remarks from teacher to student: {card.Remarks}"
                     ;

        var response = await _client.GetAnswerToPrompt("gemini-1.5-flash", "gemini-flash",
            $"You help prepare the flashcards teaching {viewModel.Deck.SourceLanguage} to students",
            prompt,
            GenerativeAiClientResponseMode.PlainText);

        card.QaSuggestionsSecondOpinion = response;
    }

    private GoogleGeminiClient GetGeminiClientInstance()
    {
        var geminiCacheFolder = viewModel.Deck.DeckPath.GeminiCacheFolder;

        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<MainWindow>()
            .AddJsonFile("secrets.json", optional: true)
            .Build();

        // Bind the configuration values to the strongly typed class
        var secrets = new SecretParameters();
        configuration.Bind(secrets);

        var logger = LoggerFactory
            .Create(builder => builder.AddConsole())
            .CreateLogger<GoogleGeminiClient>();

        var audioProvider = new GoogleGeminiClient(logger, secrets.GeminiApiKey, geminiCacheFolder);
        return audioProvider;
    }


}
