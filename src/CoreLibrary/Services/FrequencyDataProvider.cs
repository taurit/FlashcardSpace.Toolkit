using System.Text.RegularExpressions;

namespace CoreLibrary.Services;

public record FrequencyRecord(string Term, int Position, long NumOccurrences);

/// <summary>
/// Provides information about the frequency of words' occurrence in a language.
/// </summary>
public class FrequencyDataProvider
{
    private readonly Dictionary<string, FrequencyRecord> _frequencyData = new(StringComparer.OrdinalIgnoreCase);
    private readonly StringSanitizer _stringSanitizer;
    private readonly string _frequencyDictionaryFilePath;

    /// <summary>
    /// Provides information about the frequency of words' occurrence in a language.
    /// </summary>
    public FrequencyDataProvider(StringSanitizer stringSanitizer, string frequencyDictionaryFilePath)
    {
        _stringSanitizer = stringSanitizer;
        _frequencyDictionaryFilePath = frequencyDictionaryFilePath;

        if (!File.Exists(_frequencyDictionaryFilePath))
        {
            throw new FileNotFoundException($"Frequency dictionary file not found at {_frequencyDictionaryFilePath}");
        }

        LoadFrequencyData();
    }

    /// <summary>
    /// Read data from a text file (over 1,000,000 rows) and store it in memory for efficient lookup of frequency.
    ///
    /// The file contains lines in a format:
    /// {word} {number of occurrences in dataset}
    ///
    /// For example:
    /// teléfono 95015
    ///
    /// The file is sorted by the number of occurrences in descending order. This service should allow look up the position of a word in the dataset.
    /// We are not interested in the actual number of occurrences, only the position of the word in the dataset.
    /// </summary>
    /// <remarks>
    /// Todo big performance hit; I can use my favorite fast binary deserializer to improve performance probably
    /// </remarks>
    private void LoadFrequencyData()
    {
        if (_frequencyData.Any()) return;

        var lines = File.ReadAllLines(_frequencyDictionaryFilePath);
        if (!lines.Any()) throw new InvalidOperationException("Frequency dictionary file is empty.");

        // Autodetect format of frequency dictionary and load data accordingly
        var isContentInReaFormat = lines[0].Contains("Frec.normalizada");
        if (isContentInReaFormat)
            LoadFrequencyData_RealAcademiaEspañolaFormat(lines);
        else
            LoadFrequencyData_DefaultFormat(lines);

    }

    private void LoadFrequencyData_DefaultFormat(string[] lines)
    {
        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var parts = line.Split(' ');
            var word = parts[0];
            var numOccurrences = Int64.Parse(parts[1]);

            // in the dataset, duplicates are only found at the long tail (weird "words" with 1 usage like "µe"), so it's not worth to handle them
            var frequencyRecord = new FrequencyRecord(word, i, numOccurrences);
            _frequencyData.TryAdd(word, frequencyRecord);
        }
    }

    private void LoadFrequencyData_RealAcademiaEspañolaFormat(string[] lines)
    {
        // skip headers line
        const int firstLine = 1;

        // regex to match data like:
        // Orden Palabra Frec.absoluta Frec.normalizada 
        // 1.	de	9,999,518 	 65545.55 
        // 2.	la	6,277,560 	 41148.59 
        // 3.	que 	4,681,839 	 30688.85 
        Regex raeLineRegex = new Regex(@"^\s+(?<order>\d+)\.\s+(?<word>[^\s]+)\s+(?<numOccurrences>[\d,]+)\s+.*$");

        for (var i = firstLine; i < lines.Length; i++)
        {
            var line = lines[i];
            var match = raeLineRegex.Match(line);
            if (!match.Success)
                throw new InvalidOperationException($"Failed to parse line {i} ('{line}') in RAE frequency dictionary file.");

            var word = match.Groups["word"].Value;
            var numOccurrences = Int64.Parse(match.Groups["numOccurrences"].Value.Replace(",", ""));

            // in the dataset, duplicates are only found at the long tail (weird "words" with 1 usage like "µe"), so it's not worth to handle them
            var frequencyRecord = new FrequencyRecord(word, i, numOccurrences);
            _frequencyData.TryAdd(word, frequencyRecord);
        }
    }

    /// <summary>
    /// Get the position of a word in the dataset.
    /// </summary>
    /// <param name="word">The word to get the position of.</param>
    /// <returns>The position of the word in the dataset, or null if the word is not found.</returns>
    public int? GetPosition(string word)
    {
        var wordSanitizedForFrequencyCheck = SanitizeWordForFrequencyCheck(word);

        if (_frequencyData.TryGetValue(wordSanitizedForFrequencyCheck, out var frequencyRecord))
        {
            return frequencyRecord.Position;
        }

        return null;
    }

    public string SanitizeWordForFrequencyCheck(string input)
    {
        return _stringSanitizer.GetNormalizedFormOfLearnedTermWithCache(input);
    }

    public List<FrequencyRecord> Take(int numItemsToSkip, int numItemsToTake)
    {
        var subset = _frequencyData.Skip(numItemsToSkip).Take(numItemsToTake).Select(x => x.Value).ToList();
        return subset;
    }
}
