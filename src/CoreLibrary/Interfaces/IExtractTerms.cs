namespace CoreLibrary.Interfaces;

public interface IExtractTerms
{
    Task<List<TermInContext>> ExtractTerms(string inputFileName, SupportedInputLanguage sourceLanguage,
        int numItemsToSkip, int numItemsToTake);
}
