using CoreLibrary;
using CoreLibrary.Services.GenerativeAiClients;

namespace GenerateFlashcards.Services.TermExtractors;
public class FrequencyDictionaryTermExtractor(IGenerativeAiClient generativeAiClient) : IExtractTerms
{
    public async Task<List<Note>> ExtractTerms(List<string> extractedSentences)
    {
        // when working with a frequency dictionary, the sentences are just a list of words without a context.
        var words = extractedSentences;

        var result = new List<Note>();

        foreach (var word in words)
        {
            var answer = await generativeAiClient.GetAnswerToPrompt("gpt-4o-mini", "gpt-4o-mini", "You are a helpful assistant", $"What is the meaning of the word '{word}'?", false);
            var sentenceExample = answer;
            var note = new Note(word, sentenceExample, PartOfSpeech.Unknown, []);
            result.Add(note);
        }

        return result;
    }
}
