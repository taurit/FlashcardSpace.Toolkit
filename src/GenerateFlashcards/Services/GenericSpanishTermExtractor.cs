using CoreLibrary.Services.ObjectGenerativeFill;
using GenerateFlashcards.Models;
using GenerateFlashcards.Models.Spanish;

namespace GenerateFlashcards.Services;
internal class GenericSpanishTermExtractor(GenerativeFill generativeFill)
{
    public async Task<List<FlashcardNote>> ExtractTerms(string inputFileName)
    {
        var lines = await File.ReadAllLinesAsync(inputFileName);

        var modelsWithGaps = lines
            .Select(line => new SpanishTermGenericDetector { TermToCreateFlashcardFor = line })
            .ToList();

        var modelsWithGapsFilled = await generativeFill
            .FillMissingProperties(Parameters.OpenAiModelId, Parameters.OpenAiModelClassId, modelsWithGaps);

        var notes = modelsWithGapsFilled
            .Select(x => new FlashcardNote()
            {
                Term = x.FlashcardQuestionInSpanish,
                TermStandardizedForm = x.FlashcardQuestionInSpanish,
                TermStandardizedFormEnglishTranslation = x.FlashcardQuestionInEnglish,
                TermTranslation = x.FlashcardAnswerInPolish,
                TermDefinition = x.Remarks ?? "",

                Context = x.FlashcardExampleSentenceInSpanish,
                ContextEnglishTranslation = x.FlashcardExampleSentenceInEnglish,
                ContextTranslation = x.FlashcardExampleSentenceInPolish,

                Type = CoreLibrary.PartOfSpeech.Other

            })
            .ToList();

        return notes;
    }
}

