using GenerateFlashcards.Commands;
using GenerateFlashcards.Models;
using GenerateFlashcards.Services.SentenceExtractors;
using GenerateFlashcards.Services.TermExtractors;

namespace GenerateFlashcards.Services;

/// <summary>
/// Provides instances of building blocks like:
/// - <see cref="IExtractSentences"/>
/// - <see cref="IExtendNotes"/>
/// - <see cref="IGenerateOutput"/>
///
/// ... that best fit user-provided parameters (like input and output languages).
/// 
/// Some languages and alphabets benefit from having specialized implementations of these interfaces,
/// while others might be fine using the most generic one.
/// </summary>
internal class BuildingBlocksProvider(
    // Sentence extractors
    ReferenceSentenceExtractor referenceSentenceExtractor,
    FrequencyDictionarySentenceExtractor frequencyDictionarySentenceExtractor,
    AdvancedSentenceExtractor advancedSentenceExtractor,

    ReferenceTermExtractor referenceTermExtractor,
    FrequencyDictionaryTermExtractor frequencyDictionaryTermExtractor,

    ReferenceTranslator referenceTranslator
)
{

    internal IExtractSentences SelectBestSentenceExtractor(GenerateFlashcardsCommandSettings settings)
    {
        if (settings.InputFileFormat == InputFileFormat.FrequencyDictionary)
            return frequencyDictionarySentenceExtractor;

        return advancedSentenceExtractor;
    }

    public IExtractTerms SelectBestTermExtractor(GenerateFlashcardsCommandSettings settings)
    {
        if (settings.InputFileFormat == InputFileFormat.FrequencyDictionary)
            return frequencyDictionaryTermExtractor;

        return referenceTermExtractor;
    }

    public IProvideFieldValues SelectBestTranslator(GenerateFlashcardsCommandSettings settings)
    {
        return referenceTranslator;
    }
}
