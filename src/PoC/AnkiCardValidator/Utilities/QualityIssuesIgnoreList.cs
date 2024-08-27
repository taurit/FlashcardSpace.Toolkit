using CoreLibrary.Utilities;
using System.IO;

namespace AnkiCardValidator.Utilities;

/// <summary>
/// Allow adding and checking if a given quality issue is on the ignore list.
/// Ignore list is persisted into a file.
/// </summary>
internal class QualityIssuesIgnoreList
{
    private readonly string _ignoreListFilePath;

    readonly HashSet<string> _ignoredItems = [];

    public QualityIssuesIgnoreList(string ignoreListFilePath)
    {
        _ignoreListFilePath = ignoreListFilePath;
        if (File.Exists(ignoreListFilePath))
        {
            var lines = File.ReadAllLines(ignoreListFilePath);
            _ignoredItems = [.. lines];
        }
    }

    public void Add(string question, string answer, string? qualityIssue)
    {
        var hash = (question + answer + (qualityIssue ?? "null")).GetHashCodeStable();
        _ignoredItems.Add(hash);
        File.WriteAllLines(_ignoreListFilePath, _ignoredItems);
    }

    public bool IsInIgnoreList(string question, string answer, string qualityIssue)
    {
        var hash = (question + answer + qualityIssue).GetHashCodeStable();
        return _ignoredItems.Contains(hash);
    }

}
