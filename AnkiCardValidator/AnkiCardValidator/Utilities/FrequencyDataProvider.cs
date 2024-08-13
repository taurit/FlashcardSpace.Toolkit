using System.IO;

namespace AnkiCardValidator.Utilities;

/// <summary>
/// Provides information about the frequency of words' occurrence in a language.
/// </summary>
public class FrequencyDataProvider
{
    private readonly Dictionary<string, int> _frequencyData = new(StringComparer.OrdinalIgnoreCase);
    private readonly NormalFormProvider _normalFormProvider;
    private readonly string _frequencyDictionaryFilePath;

    /// <summary>
    /// Provides information about the frequency of words' occurrence in a language.
    /// </summary>
    public FrequencyDataProvider(NormalFormProvider normalFormProvider, string frequencyDictionaryFilePath)
    {
        _normalFormProvider = normalFormProvider;
        _frequencyDictionaryFilePath = frequencyDictionaryFilePath;

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
        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var parts = line.Split(' ');
            var word = parts[0];

            // in the dataset, duplicates are only found at the long tail (weird "words" with 1 usage like "µe"), so it's not worth to handle them
            _frequencyData.TryAdd(word, i);
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

        if (_frequencyData.TryGetValue(wordSanitizedForFrequencyCheck, out var position))
        {
            return position;
        }

        return null;
    }

    public string SanitizeWordForFrequencyCheck(string input)
    {
        return _normalFormProvider.GetNormalizedFormOfLearnedTermWithCache(input);
    }
}
