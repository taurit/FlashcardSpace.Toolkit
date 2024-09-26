using CoreLibrary.Models;
using CoreLibrary.Services.ObjectGenerativeFill;
using GenerateFlashcards.Models.Spanish;
using Microsoft.Extensions.Logging;

namespace GenerateFlashcards.Services;
internal class QualityControlService(GenerativeFill generativeFill, ILogger<QualityControlService> logger)
{
    public async Task<List<FlashcardNote>> AddQualitySuggestions(List<FlashcardNote> notes)
    {
        var toValidate = notes.Select(t => new FlashcardQualityImprovementSuggestion()
        {
            FrontTextSpanish = t.Term,
            BackTextPolish = t.TermTranslation,
            SentenceExampleSpanish = t.Context,
            SentenceExamplePolish = t.ContextTranslation,
        }).ToList();

        logger.LogInformation("Performing Quality Assurance of {NumCards} cards...", toValidate.Count);

        var translated = await generativeFill.FillMissingProperties(
            Parameters.OpenAiModelId, Parameters.OpenAiModelClassId, toValidate);

        if (translated.Count != notes.Count)
            throw new InvalidOperationException("Translated count does not match input count");

        var result = new List<FlashcardNote>();
        for (var index = 0; index < notes.Count; index++)
        {
            var term = notes[index];
            var termQaCheck = translated[index];

            if (term.Term != termQaCheck.FrontTextSpanish)
                throw new InvalidOperationException($"Term ({term.Term}) and repeated term in translation " +
                                                    $"({termQaCheck.FrontTextSpanish}) differ! Something went wrong.");

            var flashcardNote = term with
            {
                QaSuggestions = termQaCheck.Suggestions,
            };

            result.Add(flashcardNote);
        }

        return result;
    }
}
