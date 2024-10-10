using CoreLibrary.Services.ObjectGenerativeFill;

namespace GenerateFlashcards.Models.Spanish;

internal abstract class PartOfSpeechDetector : ObjectWithId
{
    /// <summary>
    /// An isolated word taken from the frequency dictionary, without any known context.
    /// </summary>
    public string IsolatedWord { get; init; }

    public abstract string? BaseForm { get; init; }
    public abstract string? SentenceExample { get; init; }

    // Experimental - might improve accuracy of the response and ease debugging, but is not required by the code:
    [FillWithAI]
    [FillWithAIRule($"A brief explanation of the response. If word cannot be used as a tested part of speech, explain if the word is recognized at all and what parts of speech it can be used as.")]
    public string Explanation { get; init; }
}
