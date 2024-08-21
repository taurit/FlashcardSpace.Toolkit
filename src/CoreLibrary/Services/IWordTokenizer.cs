namespace CoreLibrary.Services;

public interface IWordTokenizer
{
    List<string> GetWords(string sentence);
}
