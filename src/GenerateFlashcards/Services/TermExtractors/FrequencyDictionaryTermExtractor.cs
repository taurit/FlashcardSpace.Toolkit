using CoreLibrary;
using CoreLibrary.Services.GenerativeAiClients;

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

