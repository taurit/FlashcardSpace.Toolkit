using CoreLibrary.Interfaces;
using GenerateFlashcards.Commands;
using ReferenceWordExtractor;

namespace GenerateFlashcards.Services;

/// <summary>
/// Provides instances of building blocks like:
/// - <see cref="IExtractWords"/>
/// - <see cref="IExtendFlashcards"/>
/// - <see cref="IGenerateOutput"/>
///
/// ... that best fit user-provided parameters (like input and output languages).
/// 
/// Some languages and alphabets benefit from having specialized implementations of these interfaces,
/// while others might be fine using the most generic one.
/// </summary>
internal class BuildingBlocksProvider(
    SimpleWordExtractor simpleWordExtractor
)
{

    internal IExtractWords SelectBestWordExtractor(GenerateFlashcardsCommandSettings settings)
    {
        return simpleWordExtractor;
    }
}
