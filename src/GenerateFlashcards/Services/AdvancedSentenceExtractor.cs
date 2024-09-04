using CoreLibrary.Services;
using Microsoft.Extensions.Logging;
using WordTokenizer = CoreLibrary.Services.WordTokenizer;

namespace GenerateFlashcards.Services;

public class AdvancedSentenceExtractor(ILogger<AdvancedSentenceExtractor> logger)
{
    // instantiated here without DI so the app using this library doesn't have to register all dependencies required by this class
    readonly SentenceTokenizer _sentenceTokenizer = new(new SentenceFactory(new WordTokenizer()));

    /// <summary>
    /// Splits long text into sentences.
    /// This is a small adapter which uses `SentenceTokenizer` class I developed in some other PoC project. 
    /// </summary>
    public async Task<List<string>> ExtractSentences(string inputFileName)
    {
        logger.LogInformation("Extracting sentences from input file {InputFileName}...", inputFileName);

        var fileContent = await File.ReadAllTextAsync(inputFileName);
        var sentences = _sentenceTokenizer.TokenizeBook(fileContent);
        var extractedSentences = sentences.Select(s => s.Text).ToList();

        logger.LogInformation("Extracted {ExtractedSentencesCount} sentences", extractedSentences.Count);

        return extractedSentences;
    }
}
