using CoreLibrary.Services.GenerativeAiClients;
using Microsoft.Extensions.Logging;
using RefineDeck.ViewModels;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace RefineDeck.Utils;

internal class GeminiQualityAssuranceAgent(MainWindowViewModel viewModel)
{
    GoogleGeminiClient? _client = null;

    public async Task ValidateAllCards()
    {
        // in development, validate just current card
        var card = viewModel.SelectedFlashcard;
        if (card == null) return;

        List<ReviewedCardViewModel> cards = [card];
        foreach (var toValidate in cards)
        {
            await ValidateSelectedCard(toValidate);
        }
    }

    private async Task ValidateSelectedCard(ReviewedCardViewModel card)
    {
        _client ??= GetGeminiClientInstance();

        var dataToValidate = new DataToValidate
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
            "The term on the front side must be used in real-world language and be understood by native speakers",
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
                     "- Brief and concrete suggestions for the flashcard author in natural language (don't skip it, even if you provide JSON).,\n" +
                     "- Then, an example of the corrected flashcard data in JSON format (same structure as in input, wrapped in ```json and ``` backtick block).\n" +
                     "\n" +
                     $"Flashcard data:\n" +
                     $"```json\n" +
                     $"{jsonPayload}\n" +
                     $"```";

        var response = await _client.GetAnswerToPrompt(Parameters.GeminiModelId, Parameters.GeminiModelId,
            $"You help prepare the flashcards to teach {viewModel.Deck.SourceLanguage} vocabulary to students natively speaking {viewModel.Deck.TargetLanguage}",
            prompt,
            GenerativeAiClientResponseMode.PlainText);

        card.QaSuggestionsSecondOpinion = SplitIntoPlainTextAndJson(response);
    }

    private GoogleGeminiClient GetGeminiClientInstance()
    {
        var geminiCacheFolder = viewModel.Deck.DeckPath.GeminiCacheFolder;
        var logger = LoggerFactory
            .Create(builder => builder.AddConsole())
            .CreateLogger<GoogleGeminiClient>();

        var secrets = Parameters.LoadSecrets();
        var audioProvider = new GoogleGeminiClient(logger, secrets.GeminiApiKey, geminiCacheFolder);
        return audioProvider;
    }

    /// <summary>
    /// Transforms the input string into two parts:
    /// 1) Unstructured text (plain text) before the first code block (```json)
    /// 2) JSON part inside the first code block (```json)
    /// 
    /// </summary>
    private PlainTextAndJsonPart SplitIntoPlainTextAndJson(string input)
    {
        var plainText = "";
        var json = "";

        // use Regex to find content between ```json and ```
        var match = Regex.Match(input, @"```json(.*?)```", RegexOptions.Singleline);
        if (match.Success)
        {
            json = match.Groups[1].Value;
            plainText = input.Substring(0, match.Index);
        }
        else
        {
            plainText = input;
        }

        plainText = plainText.Trim();

        // deserialize JSON to the object
        if (!String.IsNullOrWhiteSpace(json))
        {
            try
            {
                DataToValidate suggestion = JsonSerializer.Deserialize<DataToValidate>(json);
                return new PlainTextAndJsonPart(plainText, suggestion);
            }
            catch (JsonException e)
            {
                // log the exception and continue
                plainText += $"\n\nFailed to deserialize JSON suggestion: {e.Message}\n\n" +
                             $"```\n" +
                             $"{json}\n" +
                             $"```";
            }
        }
        return new PlainTextAndJsonPart(plainText, null);
    }
}
