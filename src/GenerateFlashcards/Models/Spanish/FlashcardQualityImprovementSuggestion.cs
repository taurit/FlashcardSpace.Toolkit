using CoreLibrary.Services.ObjectGenerativeFill;
using System.ComponentModel.DataAnnotations;

namespace GenerateFlashcards.Models.Spanish;

class FlashcardQualityImprovementSuggestion : ObjectWithId
{
    [Key]
    public string FrontTextSpanish { get; init; }

    public string BackTextPolish { get; init; }

    public string SentenceExampleSpanish { get; init; }

    public string SentenceExamplePolish { get; init; }

    [FillWithAI]
    [FillWithAIRule($"This field contains suggestions for flashcard deck author on how to improve the flashcard quality.")]
    [FillWithAIRule("It helps with quality assurance oif the flashcard deck.")]
    [FillWithAIRule($"Flashcard is to be used by a native Polish student learning Spanish vocabulary.")]
    [FillWithAIRule($"If any flashcard property contains typos, grammar errors, non-existing words or other quality issues, provide brief suggestion to reject the flashcard, or how to fix the issue.")]
    [FillWithAIRule($"If the flashcard is correct and has acceptable quality, leave this field empty.")]
    public string Suggestions { get; init; }

}
