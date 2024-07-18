using AnkiCardValidator.Models;
using Scriban;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.Serialization;
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

/// <remarks>
/// When renaming properties, remember to rename in Scriban template(s), too!
/// </remarks>
internal record FlashcardToEvaluateSpanishToPolish(string FrontSide, string BackSide);

internal static class FlashcardQualityEvaluator
{
    internal static async Task<FlashcardQualityEvaluationBatchResult> EvaluateFlashcardsQuality(List<FlashcardToEvaluateSpanishToPolish> noteBatch)
    {
        const int allowedNumAttempts = 2;
        int attempt = 0;
        while (true)
        {
            attempt++;

            try
            {
                // generate prompt
                var templateContent = await File.ReadAllTextAsync(Settings.EvaluateCardQualityBatchPromptPath);
                var template = Template.Parse(templateContent, Settings.EvaluateCardQualityBatchPromptPath);
                var jsonSerializerOptions = new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
                var noteBatchSerialized = JsonSerializer.Serialize(noteBatch, jsonSerializerOptions);
                var templateInput = new FlashcardQualityEvaluationInput(noteBatchSerialized);
                var prompt = await template.RenderAsync(templateInput, x => x.Name);

                // get response
                var responseFileName = await ChatGptHelper.GetAnswerToPromptUsingChatGptApi(prompt, attempt);
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
                    throw new SerializationException($"Failed to deserialize ChatGPT response. Response is cached in {responseFileName}.");
                }
                if (evaluation.Evaluations.Count != noteBatch.Count)
                {
                    throw new ArgumentOutOfRangeException($"Number of items in output array ({evaluation.Evaluations.Count}) does not match number of items in input ({noteBatch.Count}), cannot continue. Response is cached in {responseFileName}.");
                }

                return new FlashcardQualityEvaluationBatchResult(evaluation.Evaluations, chatGptResponse);
            }
            catch (Exception)
            {
                Console.WriteLine($"ChatGPT API call failed (attempt {attempt} / {allowedNumAttempts})");
                if (attempt >= allowedNumAttempts)
                {
                    throw;
                }
            }
        }
    }
}
