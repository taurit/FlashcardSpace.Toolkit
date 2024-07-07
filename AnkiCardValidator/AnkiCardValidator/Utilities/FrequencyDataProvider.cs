using System.IO;

namespace AnkiCardValidator.Utilities;

/// <summary>
/// Provides information about the frequency of words' occurrence in a language.
/// </summary>
public class FrequencyDataProvider(string frequencyDictionaryFilePath)
{
    private readonly Dictionary<string, int> _frequencyData = new(StringComparer.OrdinalIgnoreCase);

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
    public void LoadFrequencyData()
    {
        if (_frequencyData.Any()) return;

        var lines = File.ReadAllLines(frequencyDictionaryFilePath);
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

    /// <summary>
    /// Heuristically attempts to convert a flashcard content to a word that can be looked up in the frequency dataset.
    /// Examples:
    /// - "el teléfono" -> "teléfono"
    /// - "por un lado..." -> "por un lado"
    /// - "¡Hola!" -> "hola"
    /// - "¿Cómo?" -> "cómo"
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public string SanitizeWordForFrequencyCheck(string input)
    {
        // remove punctuation
        var sanitized = new string(input.Where(c => !char.IsPunctuation(c)).ToArray());

        // remove leading/trailing whitespaces
        sanitized = sanitized.Trim();

        // lowercase
        var lowercase = sanitized.ToLowerInvariant();

        // remove preceding "el", "la", "los", "las", "un", "una", "unos", "unas"
        var wordsToRemove = new[] { "el", "la", "los", "las", "un", "una", "unos", "unas" };
        foreach (var wordToRemove in wordsToRemove)
        {
            if (lowercase.StartsWith(wordToRemove + " "))
            {
                lowercase = lowercase.Substring(wordToRemove.Length + 1);
            }
        }

        // trim what's left
        var trimmed = lowercase.Trim();

        return trimmed;
    }

}
