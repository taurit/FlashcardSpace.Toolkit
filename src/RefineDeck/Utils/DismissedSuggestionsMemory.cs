using RefineDeck.ViewModels;
using System.IO;

namespace RefineDeck.Utils;

internal class DismissedSuggestionsMemory(string dismissedSuggestionsFilePath)
{
    private readonly List<string> _dismissedSuggestionsHashes =
        File.Exists(dismissedSuggestionsFilePath) ? File.ReadAllLines(dismissedSuggestionsFilePath).ToList() : new List<string>();

    public void Dismiss(PlainTextAndJsonPart? suggestion)
    {
        if (suggestion is null) return;
        if (IsDismissed(suggestion)) return;

        _dismissedSuggestionsHashes.Add(suggestion.Fingerprint);
        File.WriteAllLines(dismissedSuggestionsFilePath, _dismissedSuggestionsHashes);
    }

    public bool IsDismissed(PlainTextAndJsonPart? dismissedSuggestion)
    {
        if (dismissedSuggestion is null) return false;

        return _dismissedSuggestionsHashes.Contains(dismissedSuggestion.Fingerprint);
    }
}
