using CoreLibrary.Services.ObjectGenerativeFill;
using GenerateFlashcards.Models.Spanish;

namespace GenerateFlashcards.Models.English;

internal class EnglishWordInContext : ObjectWithId
{
    /// <summary>
    /// A word taken from the frequency dictionary, without any known context.
    /// </summary>
    public string Word { get; init; }

    [FillWithAI]
    [FillWithAIRule("For nouns in any form (like 'cat'), the value should be noun's nominal form ('a cat').")]
    [FillWithAIRule("For verbs in any form (like 'struck'), the value should be verb's infinitive form ('to strike').")]
    [FillWithAIRule("For adjectives in any form (like 'worst'), the value should be a general term ('bad').")]
    [FillWithAIRule("If a word can serve as more than one part of speech (e.g. 'cut' can be both noun and verb), choose the more popular one.")]
    public string WordBaseForm { get; init; }

    [FillWithAI]
    [FillWithAIRule("A simple sentence demonstrating usage of the provided word in the originally provided form (not the base form). Sentence has no more than 6 words and be suitable for students at A1-A2 level.")]
    public string SentenceExample { get; init; }

    [FillWithAI]
    [FillWithAIRule($"Part of speech, consistent with the {nameof(WordBaseForm)} property.")]
    [FillWithAIRule($"The value '{nameof(EnglishPartOfSpeech.Other)}' serves as a fallback, if no other available option fits.")]
    public EnglishPartOfSpeech PartOfSpeech { get; init; }
}

