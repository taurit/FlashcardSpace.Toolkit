using System.Text.Encodings.Web;
using System.Text.Json;

namespace BookToAnki.Services;

public class LinkingExceptionsStore
{
    record LinkingException(string Word1, string Word2);

    private readonly HashSet<LinkingException> _exceptions;
    private readonly string _fileName;
    private static readonly JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    public LinkingExceptionsStore(string fileName)
    {
        _fileName = fileName;
        _exceptions = new HashSet<LinkingException>(new WordPairComparer());

        LoadExceptions();
    }

    public void AddException(string word1, string word2)
    {
        _exceptions.Add(new LinkingException(word1, word2));
        SaveExceptions();
    }

    public bool IsInExceptionList(string word1, string word2)
    {
        return _exceptions.Contains(new LinkingException(word1, word2));
    }

    private void LoadExceptions()
    {
        if (!File.Exists(_fileName))
            return;

        var json = File.ReadAllText(_fileName);
        var exceptionsList = JsonSerializer.Deserialize<List<LinkingException>>(json);

        if (exceptionsList != null)
            foreach (var exception in exceptionsList)
                _exceptions.Add(exception);
    }

    private void SaveExceptions()
    {
        var json = JsonSerializer.Serialize(_exceptions.ToList(), JsonSerializerOptions);
        File.WriteAllText(_fileName, json);
    }

    private class WordPairComparer : IEqualityComparer<LinkingException>
    {
        public bool Equals(LinkingException? x, LinkingException? y)
        {
            if (x is null || y is null) return false;

            return (x.Word1 == y.Word1 && x.Word2 == y.Word2) || (x.Word1 == y.Word2 && x.Word2 == y.Word1);
        }

        public int GetHashCode(LinkingException obj)
        {
            var hash1 = obj.Word1.GetHashCode();
            var hash2 = obj.Word2.GetHashCode();

            return hash1 ^ hash2;
        }
    }
}
