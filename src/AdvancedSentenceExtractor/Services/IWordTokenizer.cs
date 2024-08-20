namespace AdvancedSentenceExtractor.Services;

public interface IWordTokenizer
{
    List<string> GetWords(string sentence);
}
