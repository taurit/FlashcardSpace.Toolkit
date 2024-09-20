using CoreLibrary.Models;
using CoreLibrary.Services.ObjectGenerativeFill;
using GenerateFlashcards.Models.Spanish;

namespace GenerateFlashcards.Services;

internal class SpanishToPolishTranslationProvider(GenerativeFill generativeFill)
{
    public async Task<List<FlashcardNote>> AnnotateWithPolishTranslation(List<FlashcardNote> terms)
    {
        var toTranslate = terms.Select(t => new SpanishTermWithPolishTranslation()
        {
            SpanishWord = t.Term,
            SpanishWordPartOfSpeech = t.Type,
            SpanishSentence = t.Context
        }).ToList();

        var translated = await generativeFill.FillMissingProperties(
            Parameters.OpenAiModelId, Parameters.OpenAiModelClassId, toTranslate);

        if (translated.Count != terms.Count)
            throw new InvalidOperationException("Translated count does not match input count");

        var result = new List<FlashcardNote>();
        for (var index = 0; index < terms.Count; index++)
        {
            var term = terms[index];
            var termTranslation = translated[index];

            if (term.Term != termTranslation.SpanishWord)
                throw new InvalidOperationException($"Term ({term.Term}) and repeated term in translation " +
                                                    $"({termTranslation.SpanishWord}) differ! Something went wrong.");

            var flashcardNote = term with
            {
                TermTranslation = termTranslation.SpanishWordEquivalentInPolish,
                ContextTranslation = termTranslation.SpanishSentenceEquivalentInPolish
            };

            result.Add(flashcardNote);
        }

        return result;
    }
}
