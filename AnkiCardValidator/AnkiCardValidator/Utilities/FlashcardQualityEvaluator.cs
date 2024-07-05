using AnkiCardValidator.Models;
using Scriban;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AnkiCardValidator.Utilities;

/// <remarks>
/// Represents response defined in Scriban template (ChatGPT prompt), be careful when renaming any properties!
/// </remarks>
internal record FlashcardQualityEvaluationInput(string FlashcardModelSerialized);

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global", Justification = "Used in ChatGPT response deserialization")]
public record Meaning(string EnglishEquivalent, string PolishEquivalent, string Definition);

internal record FlashcardQualityEvaluation(CefrClassification CEFRClassification, string Dialect, string QualityIssues, List<Meaning> Meanings);

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
}
