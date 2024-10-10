using CoreLibrary.Models;
using CoreLibrary.Services.ObjectGenerativeFill;
using GenerateFlashcards.Models.Spanish;

namespace GenerateFlashcards.Services;

/// <remarks>
/// English translations are generated for flashcards regardless of input/output language
/// (e.g., even if creating a deck like Spanish -> Polish).
/// This is because English translations are useful when talking to AI models, e.g. generating images
/// using Stable Diffusion or other models trained on English keywords.
/// </remarks>
internal class SpanishToEnglishTranslationProvider(GenerativeFill generativeFill)
{
    public async Task<List<FlashcardNote>> AnnotateWithEnglishTranslation(List<TermInContext> terms)
    {
        var toTranslate = terms.Select(t => new SpanishTermWithEnglishTranslation()
        {
            SpanishWord = t.TermBaseForm, // TODO: BŁĄD? Czy to nie powinno być t.TermOriginal?
            SpanishWordPartOfSpeech = t.PartOfSpeech,
            SpanishSentence = t.Sentence
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

            if (term.TermBaseForm != termTranslation.SpanishWord)
                throw new InvalidOperationException($"Term ({term.TermBaseForm}) and repeated term in translation " +
                                                    $"({termTranslation.SpanishWord}) differ! Something went wrong.");

            var flashcardNote = new FlashcardNote()
            {
                Term = term.TermOriginal,
                TermBaseForm = term.TermBaseForm,
                Context = term.Sentence,
                Type = term.PartOfSpeech,

                TermStandardizedFormEnglishTranslation = termTranslation.SpanishWordEquivalentInEnglish,
                ContextEnglishTranslation = termTranslation.SpanishSentenceEquivalentInEnglish
            };

            result.Add(flashcardNote);
        }

        return result;
    }
}
