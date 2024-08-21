using CoreLibrary.Interfaces;
using GenerateFlashcards.Commands;

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
    AdvancedSentenceExtractor advancedSentenceExtractor,

    ReferenceTermExtractor referenceTermExtractor,
    ReferenceTranslator referenceTranslator
)
{

    internal IExtractSentences SelectBestSentenceExtractor(GenerateFlashcardsCommandSettings settings)
    {
        return advancedSentenceExtractor;
    }

    public IExtractTerms SelectBestTermExtractor(GenerateFlashcardsCommandSettings settings)
    {
        return referenceTermExtractor;
    }

    public IProvideFieldValues SelectBestTranslator(GenerateFlashcardsCommandSettings settings)
    {
        return referenceTranslator;
    }
}
