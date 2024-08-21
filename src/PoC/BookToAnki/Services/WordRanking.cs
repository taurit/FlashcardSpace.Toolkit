namespace BookToAnki.Services;
public class WordRanking
{
    readonly Dictionary<string, int> _numUsages = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

    public WordRanking(IEnumerable<String> words)
    {
        foreach (var word in words)
        {
            _numUsages.TryAdd(word, 0);
            _numUsages[word]++;
        }
    }

    public int HowManyUsages(string word) => _numUsages.TryGetValue(word, out var howManyUsages) ? howManyUsages : 0;
}
