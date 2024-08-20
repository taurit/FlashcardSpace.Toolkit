using AdvancedSentenceExtractor.Services;
using CoreLibrary.Interfaces;
using WordTokenizer = AdvancedSentenceExtractor.Services.WordTokenizer;

namespace AdvancedSentenceExtractor;
public class AdvancedSentenceExtractor() : IExtractSentences
{
    // instantiated here without DI so the app using this library doesn't have to register all dependencies required by this class
    readonly SentenceTokenizer _sentenceTokenizer = new(new SentenceFactory(new WordTokenizer()));

    /// <summary>
    /// Splits long text into sentences.
    /// This is a small adapter which uses `SentenceTokenizer` class I developed in some other PoC project. 
    /// </summary>
    public async Task<List<string>> ExtractSentences(string inputFileName)
    {
        var fileContent = await File.ReadAllTextAsync(inputFileName);
        var sentences = _sentenceTokenizer.TokenizeBook(fileContent);
        var sentencesAdapted = sentences.Select(s => s.Text).ToList();

        return sentencesAdapted;
    }
}
