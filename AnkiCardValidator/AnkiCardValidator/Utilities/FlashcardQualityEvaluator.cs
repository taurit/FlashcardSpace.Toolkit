using AnkiCardValidator.Models;
using Scriban;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AnkiCardValidator.Utilities;

/// <remarks>
/// Represents response defined in Scriban template (ChatGPT prompt), be careful when renaming any properties!
/// </remarks>
internal record FlashcardQualityEvaluationInput(string FlashcardModelSerialized);

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global", Justification = "Used in ChatGPT response deserialization")]
public record Meaning(string EN, string PL, string Def);

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global", Justification = "Used in ChatGPT response deserialization")]
internal record FlashcardQualityEvaluation(CefrClassification CEFR, string Issues, List<Meaning> Meanings);

/// <summary>
/// Model for ChatGPT JSON response (must be an object, not an array)
/// </summary>
internal record FlashcardQualityEvaluationBatch(List<FlashcardQualityEvaluation> Evaluations);

internal record FlashcardQualityEvaluationBatchResult(List<FlashcardQualityEvaluation> Evaluations, string RawChatGptResponse);

internal static class FlashcardQualityEvaluator
{
    internal static async Task<FlashcardQualityEvaluationBatchResult> EvaluateFlashcardsQuality(List<AnkiNote> noteBatch)
    {
        var jsonSerializerOptions = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        // generate prompt
        var templateContent = await File.ReadAllTextAsync(Settings.EvaluateCardQualityBatchPromptPath);
        var template = Template.Parse(templateContent, Settings.EvaluateCardQualityBatchPromptPath);
        var noteBatchSerialized = JsonSerializer.Serialize(noteBatch, jsonSerializerOptions);
        var templateInput = new FlashcardQualityEvaluationInput(noteBatchSerialized);
        var prompt = await template.RenderAsync(templateInput, x => x.Name);

        // get response
        var responseFileName = await ChatGptHelper.GetAnswerToPromptUsingChatGptApi(prompt);
        var chatGptResponse = await File.ReadAllTextAsync(responseFileName);

        // parse response (chatGptResponse contains JSON that can be deserialized to `FlashcardQualityEvaluation`)
        var options = new JsonSerializerOptions
        {
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
            PropertyNameCaseInsensitive = true
        };
        var evaluation = JsonSerializer.Deserialize<FlashcardQualityEvaluationBatch>(chatGptResponse, options);

        if (evaluation is null)
        {
            throw new InvalidOperationException("Failed to deserialize ChatGPT response.");
        }
        if (evaluation.Evaluations.Count != noteBatch.Count)
        {
            throw new InvalidOperationException("Number of items in output array does not match number of items in input, cannot continue.");
        }

        return new FlashcardQualityEvaluationBatchResult(evaluation.Evaluations, chatGptResponse);
    }
}
