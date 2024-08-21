using System.Text.Encodings.Web;
using System.Text.Json;

namespace BookToAnki.Services;
public class WordsLinker
{
    private readonly string? _persistenceFilePath;
    private readonly List<HashSet<string>> _groups;
    private static readonly JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    private readonly Dictionary<string, HashSet<string>> quickGroupLookup;

    public WordsLinker(string? persistenceFilePath = null)
    {
        _persistenceFilePath = persistenceFilePath;

        if (_persistenceFilePath is null || !File.Exists(_persistenceFilePath))
        {
            _groups = new List<HashSet<string>>();
        }
        else
        {
            var serializer = File.OpenRead(_persistenceFilePath);
            var deserialized = JsonSerializer.Deserialize<List<HashSet<string>>>(serializer);
            _groups = deserialized ?? throw new ArgumentException($"The file '{persistenceFilePath}' could not be deserialized.");
        }

        quickGroupLookup = new Dictionary<string, HashSet<string>>();
        foreach (var group in _groups)
        {
            foreach (var word in group)
            {
                quickGroupLookup[word] = group;
            }
        }
    }

    private HashSet<string>? FindGroupContainingWord(string word)
    {
        if (quickGroupLookup.TryGetValue(word, out var containingWord)) return containingWord;

        // use base structure instead (slower, but tested):
        //foreach (var group in _groups)
        //{
        //    if (group.Contains(word))
        //    {
        //        return group;
        //    }
        //}
        return null;
    }

    public void LinkWords(string word1, string word2)
    {
        var group1 = FindGroupContainingWord(word1);
        var group2 = FindGroupContainingWord(word2);

        if (group1 is null && group2 is null)
        {
            var newGroup = new HashSet<string> { word1, word2 };
            _groups.Add(newGroup);
            quickGroupLookup[word1] = newGroup;
            quickGroupLookup[word2] = newGroup;
        }
        else if (group1 is null && group2 is not null)
        {
            group2.Add(word1);
            quickGroupLookup[word1] = group2;
        }
        else if (group1 is not null && group2 is null)
        {
            group1.Add(word2);
            quickGroupLookup[word2] = group1;
        }
        else if (group1 is not null && group2 is not null)
        {
            // merge groups
            foreach (var elementFromGroup2 in group2)
            {
                group1.Add(elementFromGroup2);
                quickGroupLookup[elementFromGroup2] = group1;
            }
            _groups.Remove(group2);

        }

        PersistIfNeeded();
    }

    public void UnlinkWord(string word)
    {
        var group = FindGroupContainingWord(word);
        if (group is not null)
        {
            group.Remove(word);

            if (quickGroupLookup[word].Count == 1) quickGroupLookup.Remove(quickGroupLookup[word].Single());
            quickGroupLookup.Remove(word);

            if (group.Count == 1) _groups.Remove(group);
        }
        PersistIfNeeded();
    }

    public bool ToggleWord(string wordToToggle, string selectedWord)
    {
        var alreadyLinked = AreWordsLinked(wordToToggle, selectedWord);
        if (alreadyLinked)
        {
            UnlinkWord(wordToToggle);
        }
        else
        {
            LinkWords(wordToToggle, selectedWord);
        }

        PersistIfNeeded();
        return AreWordsLinked(wordToToggle, selectedWord);
    }

    public bool IsWordLinkedWithAnyOther(string word)
    {
        var group = FindGroupContainingWord(word);
        return group is not null;
    }


    public bool AreWordsLinked(string word1, string word2)
    {
        var group = FindGroupContainingWord(word1);
        return group is not null && group.Contains(word2);
    }

    public IEnumerable<string> GetAllLinkedWords(string word)
    {
        var group = FindGroupContainingWord(word);
        if (group is null)
            return Array.Empty<string>();

        return group;
    }

    private void PersistIfNeeded()
    {
        if (_persistenceFilePath is null) return;
        var serialized = JsonSerializer.Serialize(_groups, JsonSerializerOptions);
        File.WriteAllText(_persistenceFilePath, serialized);
    }

}
