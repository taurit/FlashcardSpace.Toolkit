using CoreLibrary;

namespace GenerateFlashcards.Services.TermExtractors;
public class FrequencyDictionaryTermExtractor : IExtractTerms
{
    public FrequencyDictionaryTermExtractor()
    {

    }

    public Task<List<Note>> ExtractTerms(List<string> extractedSentences)
    {
        // when working with a frequency dictionary, the sentences are just a list of words without a context.
        var words = extractedSentences;

        var result = new List<Note>();

        foreach (var word in words)
        {
            var sentenceExample = "GENERATED SENTENCE EXAMPLE";
            var note = new Note(word, sentenceExample, PartOfSpeech.Unknown, []);
            result.Add(note);
        }

        return Task.FromResult(result);
    }
}
