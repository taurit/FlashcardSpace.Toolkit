namespace CoreLibrary.Interfaces;

public interface IExtractTerms
{
    Task<List<Note>> ExtractTerms(List<string> extractedSentences);
}
