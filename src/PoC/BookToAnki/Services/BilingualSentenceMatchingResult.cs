using BookToAnki.Models;
using MemoryPack;

namespace BookToAnki.Services;

[MemoryPackable]
public partial record BilingualSentenceMatchingResult
{
    public BilingualSentenceMatchingResult(List<BilingualSentence> MatchedSentences,
        float SuccessRatePercent)
    {
        this.MatchedSentences = MatchedSentences;
        this.SuccessRatePercent = SuccessRatePercent;

        this.MatchedSentencesLookup = MatchedSentences.ToLookup(x => x.PrimaryLanguage, z => z.SecondaryLanguage);
        this.MatchedSentencesLookupReverse = MatchedSentences.ToLookup(x => x.SecondaryLanguage, z => z.PrimaryLanguage);
    }

    [MemoryPackIgnore]
    public ILookup<string, string> MatchedSentencesLookup { get; init; }
    [MemoryPackIgnore]
    public ILookup<string, string> MatchedSentencesLookupReverse { get; set; }

    public List<BilingualSentence> MatchedSentences { get; init; }
    public float SuccessRatePercent { get; init; }

}
