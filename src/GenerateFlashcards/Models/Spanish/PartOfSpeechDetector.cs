using CoreLibrary.Services.ObjectGenerativeFill;

namespace GenerateFlashcards.Models.Spanish;

internal abstract class PartOfSpeechDetector : ObjectWithId
{
    /// <summary>
    /// An isolated word taken from the frequency dictionary, without any known context.
    /// </summary>
    public string IsolatedWord { get; init; }

    public abstract string? BaseForm { get; init; }

    [FillWithAI]
    [FillWithAIRule($"A brief sentence example with the word in the form given in {nameof(IsolatedWord)} .")]
    [FillWithAIRule("Use A1-B1 level vocabulary suitable for wide range of student.")]
    [FillWithAIRule("The sentence should describe a scene that can be easily depicted in a picture or vividly imagined.")]
    [FillWithAIRule("Avoid abstract words and statements.")]
    [FillWithAIRule($"If {nameof(BaseForm)} is null, this value is also null.")]
    public string? SentenceExample { get; init; }

    // Experimental - might improve accuracy of the response and ease debugging, but is not required by the code:
    [FillWithAI]
    [FillWithAIRule($"A brief explanation of the response")]
    public string Explanation { get; init; }
}
