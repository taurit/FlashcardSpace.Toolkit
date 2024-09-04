namespace CoreLibrary.Interfaces;

public interface IExtractTerms
{
    Task<List<TermInContext>> ExtractTerms(List<string> extractedSentences, string contentLanguageName);
}
