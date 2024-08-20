using BookToAnki.Models;

namespace BookToAnki.Services;
public class SentenceFactory
{
    public SentenceFactory(IWordTokenizer wordTokenizer)
    {
        _wordTokenizer = wordTokenizer;
    }

    private readonly IWordTokenizer _wordTokenizer;

    public Sentence BuildSentence(string text, Sentence? previousSentence)
    {
        var words = _wordTokenizer.GetWords(text);
        return new Sentence(text, words, previousSentence);
    }
}
