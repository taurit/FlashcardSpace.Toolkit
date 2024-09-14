using CoreLibrary.Services.ObjectGenerativeFill;
using System.Diagnostics;

namespace GenerateFlashcards.Models.Spanish;

[DebuggerDisplay("#{Id} {IsolatedWord} ({PossiblePartsOfSpeechUsage.Count} roles)")]
internal class SpanishWordPartsOfSpeech : ObjectWithId
{
    /// <summary>
    /// An isolated word taken from the frequency dictionary, without any known context.
    /// </summary>
    public string IsolatedWord { get; init; }

    [FillWithAI]
    [FillWithAIRule($"List ALL possible parts of speech that the {nameof(IsolatedWord)} can take in grammatically correct Spanish sentences.")]
    [FillWithAIRule("Include EVERY possible part of speech, even if the word is more commonly used in one form.")]
    [FillWithAIRule("If the word can be used in different parts of speech (e.g., as a Sustantivo, Adjetivo, or Verbo), list each of them separately.")]
    [FillWithAIRule($"If the given word is a non-existing word or part of speech can't be matched, return an empty array.")]
    public List<PossiblePartOfSpeech> PossiblePartsOfSpeechUsage { get; init; }

    /// <summary>
    /// The following properties are an attempt to steer the model towards a more accurate answer.
    /// It has difficulties listing all possible parts of speech for a given word.
    /// </summary>

    [FillWithAI]
    [FillWithAIRule("Set to true if the word can be used as a verb in ANY context, even if it's not the most common usage.")]
    public bool CanServeAsVerbo { get; init; }

    [FillWithAI]
    [FillWithAIRule("Set to true if the word can be used as a noun in ANY context, even if it's not the most common usage.")]
    public bool CanServeAsSustantivo { get; init; }

    [FillWithAI]
    [FillWithAIRule("Set to true if the word can be used as an adjective in ANY context, even if it's not the most common usage.")]
    public bool CanServeAsAdjetivo { get; init; }


    [FillWithAI]
    [FillWithAIRule("List ALL parts of speech this word can serve as, separated by commas. Include even uncommon usages.")]
    public string AllPossiblePartsOfSpeech { get; init; }


    //[FillWithAI]
    //[FillWithAIRule($"Provide a brief explanation of ALL the parts of speech listed in {nameof(PossiblePartsOfSpeechUsage)} and why they are included.")]
    //[FillWithAIRule("If any part of speech is not included, explain why it was omitted.")]
    //public string Explanation { get; init; }
}

internal class PossiblePartOfSpeech
{
    [FillWithAI]
    [FillWithAIRule($"The part of speech that the '{nameof(SpanishWordPartsOfSpeech.IsolatedWord)}' (in its exact form) can serve as.")]
    public SpanishPartOfSpeech PartOfSpeech { get; init; }

    [FillWithAI]
    [FillWithAIRule("For Sustantivo (e.g., 'gatos', 'manta'), contains the singular form of the word, always with a definite article ('el gato', 'la manta')")]
    [FillWithAIRule("For Verbos (e.g., 'vive'), contains the infinitive form ('vivir').")]
    [FillWithAIRule("For Adjectivos (e.g., 'verdes', 'caras'), contains the singular masculine form ('verde', 'caro').")]
    [FillWithAIRule("For other parts of speech, use the most commonly recognized base form.")]
    public string WordBaseForm { get; init; }

    [FillWithAI]
    [FillWithAIRule($"A brief sentence example showing the word in {nameof(SpanishWordPartsOfSpeech.IsolatedWord)} functioning as the part of speech.")]
    [FillWithAIRule("Prefer A1-B1 level vocabulary suitable for wide range of student.")]
    [FillWithAIRule("The sentence should describe a scene that can be easily depicted in a picture or vividly imagined.")]
    [FillWithAIRule("Avoid abstract words and statements.")]
    [FillWithAIRule("Avoid idiomatic expressions. Focus on direct, everyday usage.")]
    public string SentenceExample { get; init; }
}

