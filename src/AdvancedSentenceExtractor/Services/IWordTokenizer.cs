namespace BookToAnki.Services;

public interface  IWordTokenizer
{
    List<string> GetWords(string sentence);
}
