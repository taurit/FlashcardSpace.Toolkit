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

        //var cards = viewModel.Deck.Flashcards;
        foreach (var toValidate in cards)
        {
            await ValidateSelectedCard(toValidate);
        }
    }

    private async Task ValidateSelectedCard(ReviewedCardViewModel card)
    {
        var sourceLanguage = viewModel.Deck.SourceLanguage;
        var targetLanguage = viewModel.Deck.TargetLanguage;

        _client ??= GetGeminiClientInstance();

        var dataToValidate = new DataToValidate
        {
            FrontSide_Question = card.Term,
            BackSide_Answer = card.TermTranslation,

            SentenceExample = card.SentenceExample,
            SentenceExampleTranslation = card.SentenceExampleTranslation,

            RemarksFromTeacherToStudent = card.Remarks
        };

        var jsonSerializerOptions = new JsonSerializerOptions()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };
        string jsonPayload = JsonSerializer.Serialize(dataToValidate, jsonSerializerOptions);

        string[] rules = new[]
        {
            "Ensure all text is free of spelling and grammatical errors.",

            $"The question in {nameof(DataToValidate.FrontSide_Question)} should be in {sourceLanguage}, represent a term used in real-world conversations, and be easily understood by native speakers.",
            $"The answer in {nameof(DataToValidate.BackSide_Answer)} must be in {targetLanguage} and accurately reflect the most appropriate meaning of the term from {nameof(DataToValidate.FrontSide_Question)}.",
            $"{nameof(DataToValidate.SentenceExample)} should be in {sourceLanguage} and, should include the term from {nameof(DataToValidate.FrontSide_Question)}.",
            $"{nameof(DataToValidate.SentenceExampleTranslation)} should be a translation of {nameof(DataToValidate.SentenceExample)} to {targetLanguage}. It should be accurate and ideally, follow same order of words as {nameof(DataToValidate.SentenceExample)}, if rules of grammar allow it.",
            $"{nameof(DataToValidate.RemarksFromTeacherToStudent)} should only be included if the term requires additional clarification, such as being a grammatical exception or having multiple meanings that might be confusing. If present, the value should be in {targetLanguage}.",
            "If the flashcard is fully correct, respond only with \"OK\"."
        };
        var rulesString = String.Join("\n", rules.Select(x => $"- {x}"));

        var prompt = "Validate the correctness and quality of the flashcard provided below using the following rules:\n" +
                     rulesString +
                     "\n" +
                     "\n" +
                     "If any part of the flashcard requires modification for correctness or clarity, provide:\n" +
                     "\n" +
                     "- Provide a brief explanation in English of why a change is needed (skip this if everything is correct).\n" +
                     "- Show a corrected version of the flashcard in JSON format, wrapped in a ```json block. Modify only the incorrect parts, without regenerating the entire content.\n" +
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
