namespace CoreLibrary.Interfaces;

public record ExtractedWord(string Word, List<string> SentencesWhereWordIsFound);

public interface IExtractWords
{
    Task<List<ExtractedWord>> ExtractWords(string inputFileName);
}
