using CoreLibrary.Services.ObjectGenerativeFill;

namespace GenerateFlashcards.Models.Spanish;

internal class SpanishWordPartsOfSpeech : ObjectWithId
{
    /// <summary>
    /// An isolated word taken from the frequency dictionary, without any known context.
    /// </summary>
    public string IsolatedWord { get; init; }

    [FillWithAI]
    [FillWithAIRule($"All the parts of speech that the isolated word specified in {nameof(IsolatedWord)} could represent in a real-world sentence (Noun, Verb, Adjective, etc.).")]
    [FillWithAIRule("If the given word can be used in in different roles (e.g. as a noun or a verb, depending on the context), each of them is listed.")]
    [FillWithAIRule($"If the given word is a non-existing word, return empty array.")]
    public List<PossiblePartOfSpeech> PossiblePartsOfSpeechUsage { get; init; }

    /// <summary>
    /// The following properties are an attempt to steer the model towards a more accurate answer.
    /// It has difficulties listing all possible parts of speech for a given word.
    /// </summary>
    //[FillWithAI]
    //[FillWithAIRule($"A brief explanation of why {nameof(PossiblePartsOfSpeechUsage)} doesn't contain more or less elements.")]
    //public string Explanation { get; init; }

    [FillWithAI]
    public bool CanBeUsedAsVerb { get; init; }
    [FillWithAI]
    public bool CanBeUsedAsNoun { get; init; }
    [FillWithAI]
    public bool CanBeUsedAsAdjective { get; init; }
}

internal class PossiblePartOfSpeech
{
    [FillWithAI]
    [FillWithAIRule($"Identify the part of speech that the word can serve as. Use only provided values.")]
    [FillWithAIRule($"Use '{nameof(DetectedPartOfSpeech.Other)}' as a fallback if no value fits.")]
    public DetectedPartOfSpeech PartOfSpeech { get; init; }

    [FillWithAI]
    [FillWithAIRule("For nouns (e.g., 'gatos'), provide the singular form with a definite article ('el gato').")]
    [FillWithAIRule("For verbs (e.g., 'vive'), provide the infinitive form ('vivir').")]
    [FillWithAIRule("For adjectives (e.g., 'verdes'), provide the singular masculine form ('verde').")]
    [FillWithAIRule("For other parts of speech, use the most commonly recognized base form.")]
    public string WordBaseForm { get; init; }

    [FillWithAI]
    [FillWithAIRule($"A brief sentence example with the exact word specified in {nameof(SpanishWordPartsOfSpeech.IsolatedWord)} serving as a part of speech in {nameof(PartOfSpeech)} property.")]
    [FillWithAIRule("Prefer A1-B1 level vocabulary suitable for wide range of student.")]
    [FillWithAIRule("The sentence should describe a scene that can be easily depicted in a picture or vividly imagined.")]
    [FillWithAIRule("Avoid abstract words and statements.")]
    [FillWithAIRule("Avoid idiomatic expressions. Focus on direct, everyday usage.")]
    public string SentenceExample { get; init; }
}

