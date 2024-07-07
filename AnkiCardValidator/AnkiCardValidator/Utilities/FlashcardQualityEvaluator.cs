using AnkiCardValidator.Models;
using Scriban;
using System.Diagnostics;
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

internal record FlashcardQualityEvaluation(CefrClassification CEFR, string Issues, List<Meaning> Meanings);
internal record FlashcardQualityEvaluationBatch(List<FlashcardQualityEvaluation> Evaluations);

internal static class FlashcardQualityEvaluator
{
    internal static async Task<(FlashcardQualityEvaluation? evaluation, string chatGptResponse)> EvaluateFlashcardQuality(AnkiNote note)
    {
        // generate prompt
        var templateContent = await File.ReadAllTextAsync(Settings.EvaluateCardQualityPromptPath);
        var template = Template.Parse(templateContent, Settings.EvaluateCardQualityPromptPath);
        var templateInput = new FlashcardQualityEvaluationInput(JsonSerializer.Serialize(note));
        var prompt = await template.RenderAsync(templateInput, x => x.Name);

        Debug.WriteLine(prompt);

        // get response
        var responseFileName = await ChatGptHelper.GetAnswerToPromptUsingChatGptApi(prompt);
        var chatGptResponse = await File.ReadAllTextAsync(responseFileName);

        // parse response (chatGptResponse contains JSON that can be deserialized to `FlashcardQualityEvaluation`)
        var options = new JsonSerializerOptions
        {
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
            PropertyNameCaseInsensitive = true
        };
        var evaluation = JsonSerializer.Deserialize<FlashcardQualityEvaluation>(chatGptResponse, options);

        return (evaluation, chatGptResponse);
    }

    internal static async Task<(List<FlashcardQualityEvaluation?> evaluation, string chatGptResponse)> EvaluateFlashcardsQuality(List<AnkiNote> noteBatch)
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

        Debug.WriteLine(prompt);

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

        return (evaluation.Evaluations, chatGptResponse);
    }
}
