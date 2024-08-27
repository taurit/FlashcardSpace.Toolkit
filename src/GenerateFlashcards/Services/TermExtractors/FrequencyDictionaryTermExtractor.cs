using CoreLibrary;
using CoreLibrary.Services.GenerativeAiClients;
using CoreLibrary.Services.ObjectGenerativeFill;

namespace GenerateFlashcards.Services.TermExtractors;
public class FrequencyDictionaryTermExtractor(IGenerativeAiClient generativeAiClient) : IExtractTerms
{
    public async Task<List<Note>> ExtractTerms(List<string> extractedSentences, string contentInputLanguage)
    {
        // when working with a frequency dictionary, the sentences are just a list of words without a context.
        var words = extractedSentences;

        var result = new List<Note>();

        foreach (var word in words)
        {
            var answer = await generativeAiClient.GetAnswerToPrompt(
                Parameters.OpenAiModelId,
                Parameters.OpenAiModelClassId,
                "You are a helpful assistant. If the prompt is clear, provide succinct answer without any unnecessary explanations. " +
                "If the prompt is unclear, respond asking for clarification.",
                $"Generate example of simple sentence at A1 language learning level that uses {contentInputLanguage} term: '{word}'. Sentence should have 5 words or less if possible.",
                GenerativeAiClientResponseMode.PlainText);
            var sentenceExample = answer;
            var note = new Note(word, sentenceExample, PartOfSpeech.Unknown, []);
            result.Add(note);
        }

        return result;
    }
}

internal class EnglishWordInContext : ObjectWithId
{
    string Word { get; init; }

    [FillWithAI]
    [FillWithAIRule("For nouns in any form (like 'cat'), the value should be noun's nominal form ('a cat').")]
    [FillWithAIRule("For verbs in any form (like 'struck'), the value should be verb's infinitive form ('to strike').")]
    [FillWithAIRule("For adjectives in any form (like 'worst'), the value should be a general term ('bad').")]
    string WordBaseForm { get; init; }

    [FillWithAI]
    [FillWithAIRule("A simple sentence demonstrating usage of the provided word in the originally provided form (not the base form). Sentence has no more than 6 words and be suitable for students at A1-A2 level.")]
    string SentenceExample { get; init; }

    /// <summary>
    ///    Unknown, Noun, Verb, Adjective, Idiom, Other
    /// </summary>
    string PartOfSpeech { get; init; }

}
