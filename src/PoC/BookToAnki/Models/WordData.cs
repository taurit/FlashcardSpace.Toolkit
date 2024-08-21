namespace BookToAnki.Models;

public record WordData(string Word, int Occurrences, List<WordUsageExample> UsageExamples)
{
    public static WordData operator +(WordData wd1, WordData wd2)
    {
        if (wd1 == null || wd2 == null)
            throw new ArgumentNullException();

        if (!wd1.Word.Equals(wd2.Word, StringComparison.InvariantCultureIgnoreCase))
            throw new ArgumentException("WordData can only be combined if `Word` values are identical");

        var preferredCasing = GetPreferredCasing(wd1, wd2);

        // Sum of occurrences
        int newOccurrences = wd1.Occurrences + wd2.Occurrences;

        // Union of UsageExamples
        List<WordUsageExample> newUsageExamples = wd1.UsageExamples
            .Concat(wd2.UsageExamples)
            .GroupBy(x => x.Sentence.Text)
            .Select(x => new WordUsageExample(
                preferredCasing,
                x.First().Sentence,
                x.SelectMany(z => z.TranscriptMatches).ToList(),
                x.Select(z => z.SentenceMachineTranslationPolish).FirstOrDefault(y => y is not null),
                x.Select(z => z.SentenceMachineTranslationEnglish).FirstOrDefault(y => y is not null),
                x.Select(z => z.SentenceHumanTranslationPolish).FirstOrDefault(y => y is not null),
                x.Select(z => z.SentenceHumanTranslationEnglish).FirstOrDefault(y => y is not null),
                x.Select(z => z.PolishTranslationOfTheWordNominative).FirstOrDefault(y => y is not null)
                )
            )
            .ToList();

        return new WordData(preferredCasing, newOccurrences, newUsageExamples);
    }

    public static string GetPreferredCasing(WordData wd1, WordData wd2)
    {
        // Words can differ in cases: Місяць, місяць.
        // Sometimes they have the same meaning, sometimes not.
        // Since this is not distinguishable without knowing context and meaning, I decide to merge such words
        // and prefer lowercase variant, if it was used at least once.
        var preferredCasing = wd1.Occurrences > wd2.Occurrences ? wd1.Word : wd2.Word;
        if (wd1.Word.ToLowerInvariant() == wd1.Word) preferredCasing = wd1.Word;
        else if (wd2.Word.ToLowerInvariant() == wd2.Word) preferredCasing = wd2.Word;
        return preferredCasing;
    }
}
