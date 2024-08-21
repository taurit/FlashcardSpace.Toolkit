namespace BookToAnki.Models;

public record WordOccurrences(string Word, List<WordUsageExample> UsageExamples)
{
    public int NumOccurrences => UsageExamples.Count;
}
