namespace CoreLibrary.Interfaces;

public interface IExtractSentences
{
    Task<List<string>> ExtractSentences(string inputFileName);
}
