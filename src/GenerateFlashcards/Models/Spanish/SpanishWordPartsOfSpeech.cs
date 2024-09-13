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
    [FillWithAIRule("If the given word is a non-existing word, leave the list empty.")]
    public List<PossiblePartOfSpeech> PossiblePartsOfSpeechUsage { get; init; }

    /// <summary>
    /// Interesting: asking for explanation fixed the issue with the model returning non-complete lists. Even gpt-4o-mini now works correctly.
    /// Explanation is not used for anything else than debugging AND improving the quality of the answers.
    /// </summary>
    [FillWithAI]
    [FillWithAIRule($"A brief explanation of why the {nameof(PossiblePartsOfSpeechUsage)} was filled this way, and doesn't contain more elements.")]
    public string Explanation { get; init; }
}

internal class PossiblePartOfSpeech
{
    [FillWithAI]
    [FillWithAIRule($"Identify the part of speech that the word can serve as. Use only provided values.")]
    [FillWithAIRule($"Use '{nameof(DetectedPartOfSpeech.Other)}' as a fallback if no value fits.")]
    public DetectedPartOfSpeech PartOfSpeech { get; init; }

    [FillWithAI]
    [FillWithAIRule("For nouns (e.g., 'gatos'), provide the singular form with its appropriate article ('el gato').")]
    [FillWithAIRule("For verbs (e.g., 'vive'), provide the infinitive form ('vivir').")]
    [FillWithAIRule("For adjectives (e.g., 'verdes'), provide the singular masculine form ('verde').")]
    [FillWithAIRule("Use the most commonly recognized base form.")]
    public string WordBaseForm { get; init; }

    [FillWithAI]
    [FillWithAIRule($"A brief (max 6 words), specific sentence with the exact word specified in {nameof(SpanishWordPartsOfSpeech.IsolatedWord)} property (in its specific form).")]
    [FillWithAIRule("Prefer A1-A2 level vocabulary suitable for learners.")]
    [FillWithAIRule("Avoid complex or idiomatic expressions. Focus on direct, everyday usage.")]
    [FillWithAIRule("The sentence should describe a scene that can be easily visualized and depicted in a picture. Avoid any abstract or vague statements.")]
    public string SentenceExample { get; init; }
}

