using CoreLibrary.Services.GenerativeAiClients;
using CoreLibrary.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RefineDeck.ViewModels;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace RefineDeck.Utils;

internal class GeminiQualityAssuranceAgent(MainWindowViewModel viewModel)
{
    GoogleGeminiClient? _client = null;

    public async Task ValidateSelectedCard()
    {
        _client ??= GetGeminiClientInstance();

        var card = viewModel.SelectedFlashcard;
        if (card is null) return;

        var dataToValidate = new
        {
            FrontSide_QuestionInSpanish = card.Term,
            BackSide_AnswerInPolish = card.TermTranslation,
            BackSide_SentenceExampleInSpanish = card.SentenceExample,
            BackSide_SentenceExampleTranslationToPolish = card.SentenceExampleTranslation,
            BackSide_RemarksFromTeacherToStudent = card.Remarks
        };

        var jsonSerializerOptions = new JsonSerializerOptions()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };
        string jsonPayload = JsonSerializer.Serialize(dataToValidate, jsonSerializerOptions);

        string[] rules = new[]
        {
            "Ensure all text is free of spelling and grammatical errors.",
            "The answer on the back side must be the correct and most appropriate answer to the question on the front side.",
            "Remarks to the student should only be included if the term requires additional clarification, such as being a grammatical exception or having multiple meanings that might be confusing.",
            "If the flashcard is correct and needs no changes, respond only with \"OK\" to avoid unnecessary suggestions."
        };
        var rulesString = String.Join("\n", rules.Select(x => $"- {x}"));

        var prompt = "Validate the correctness and quality of the flashcard provided below using the following rules:\n" +
                     rulesString +
                     "\n" +
                     "\n" +
                     "If any part of the flashcard requires modification for correctness or clarity, provide:\n" +
                     "\n" +
                     "- Brief and concrete suggestions for the flashcard author in natural language.,\n" +
                     "- An example of the corrected flashcard data in JSON format.\n" +
                     "\n" +
                     $"Flashcard data:\n" +
                     $"```json\n" +
                     $"{jsonPayload}\n" +
                     $"```";

        var response = await _client.GetAnswerToPrompt("gemini-1.5-flash", "gemini-flash",
            $"You help prepare the flashcards to teach {viewModel.Deck.SourceLanguage} vocabulary to students natively speaking {viewModel.Deck.TargetLanguage}",
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
