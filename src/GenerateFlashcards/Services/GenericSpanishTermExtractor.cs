using CoreLibrary.Models;
using CoreLibrary.Services.ObjectGenerativeFill;
using GenerateFlashcards.Models.Spanish;
using Microsoft.Extensions.Logging;

namespace GenerateFlashcards.Services;
internal class GenericSpanishTermExtractor(GenerativeFill generativeFill, ILogger<GenericSpanishTermExtractor> logger)
{
    public async Task<List<FlashcardNote>> ExtractTerms(string inputFileName)
    {
        var lines = (await File.ReadAllLinesAsync(inputFileName))
            .Where(line => !string.IsNullOrWhiteSpace(line))
            ;

        var modelsWithGaps = lines
            .Select(line => new SpanishTermGenericDetector { TermToCreateFlashcardFor = line })
            .ToList();

        logger.LogInformation("Filling gaps in {Count} models, this might take a while...", modelsWithGaps.Count);
        var modelsWithGapsFilled = await generativeFill
            .FillMissingProperties(Parameters.OpenAiModelId, Parameters.OpenAiModelClassId, modelsWithGaps);

        var notes = modelsWithGapsFilled
            .Select(x => new FlashcardNote()
            {
                Term = x.FlashcardQuestionInSpanish,
                TermStandardizedForm = x.FlashcardQuestionInSpanish,
                TermStandardizedFormEnglishTranslation = x.FlashcardQuestionInEnglish,
                TermTranslation = x.FlashcardAnswerInPolish,
                Remarks = x.Remarks ?? "",

                Context = x.FlashcardExampleSentenceInSpanish,
                ContextEnglishTranslation = x.FlashcardExampleSentenceInEnglish,
                ContextTranslation = x.FlashcardExampleSentenceInPolish,

                Type = CoreLibrary.PartOfSpeech.Other

            })
            .ToList();

        return notes;
    }
}

