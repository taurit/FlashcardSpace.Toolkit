namespace BookToAnki.Services;

public record PartOfSpeech(string Name, int Count)
{
    public string DisplayText => $"{Name} ({Count})";
}

public class PartOfSpeechDictionary
{
    public IReadOnlyDictionary<string, string> WordToPartOfSpeech { get; }
    public IReadOnlyList<PartOfSpeech> PartsOfSpeech { get; }

    public PartOfSpeechDictionary(Dictionary<string, string> wordToPartOfSpeech)
    {
        WordToPartOfSpeech = wordToPartOfSpeech.AsReadOnly();
        PartsOfSpeech = wordToPartOfSpeech
            .GroupBy(x => x.Value)
            .Select(x => new PartOfSpeech(x.Key, x.Count()))
            .ToList()
            .AsReadOnly();
    }
}

public class PartOfSpeechDictionaryBuilder
{
    private static readonly char[] LabelTrimChars = { '#', ' ' };

    public PartOfSpeechDictionary BuildPartOfSpeechDictionary(string inputFileContent)
    {
        var wordToPartOfSpeech = new Dictionary<string, string>();

        var lines = inputFileContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .Where(x => !String.IsNullOrEmpty(x))
            .Where(x => !x.StartsWith("```"))
            ;

        // Process each line
        string currentLabel = "Unknown";
        foreach (var line in lines)
        {
            if (line.StartsWith("#"))
            {
                currentLabel = line.Trim(LabelTrimChars);
            }
            else
            {
                if (wordToPartOfSpeech.ContainsKey(line) && wordToPartOfSpeech[line] != currentLabel)
                {
                    Console.WriteLine($"The word '{line}' has more than 1 part of speech assigned: {wordToPartOfSpeech[line]}, {currentLabel}. Correct your input data.");
                    // throw exception if I want it perfect, but most of the times I don't are - it's just a rough filter for convenience
                }
                else
                {
                    wordToPartOfSpeech[line] = currentLabel;
                }
            }
        }

        return new PartOfSpeechDictionary(wordToPartOfSpeech);
    }

    public PartOfSpeechDictionary BuildPartOfSpeechDictionaryFromFile(string fileName)
    {
        var fileContent = File.ReadAllText(fileName);
        return BuildPartOfSpeechDictionary(fileContent);
    }
}
