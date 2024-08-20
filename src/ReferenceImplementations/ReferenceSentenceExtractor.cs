using CoreLibrary.Interfaces;

namespace ReferenceImplementations;

/// <summary>
/// An oversimplified implementation of the IExtractSentences interface.
/// It's provided to demonstrate the interface and for eventual use in tests, but not
/// for production use where there are more edge cases to handle.
/// </summary>
public class ReferenceSentenceExtractor : IExtractSentences
{
    public async Task<List<string>> ExtractSentences(string inputFileName)
    {
        var inputFileContent = await File.ReadAllTextAsync(inputFileName);
        var sentences = inputFileContent.Split(['.', '!', '?', '¡'], StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();
        return sentences;
    }
}
