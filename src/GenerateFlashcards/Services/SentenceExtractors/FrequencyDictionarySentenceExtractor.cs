namespace GenerateFlashcards.Services.SentenceExtractors;

internal class FrequencyDictionarySentenceExtractor : IExtractSentences
{
    public async Task<List<string>> ExtractSentences(string inputFileName)
    {
        // The input is just a list of words. There are no sentences, but we adapt the input to the interface.
        var lines = await File.ReadAllLinesAsync(inputFileName);
        return lines.ToList();
    }
}
