using CoreLibrary.Interfaces;
using System.Diagnostics;

namespace BookToAnki.Models;


[DebuggerDisplay("{Word}: {Sentence.Text} ({QualityPenalty})")]
public class WordUsageExample(string word,
    Sentence Sentence,
    List<SentenceWithSound> TranscriptMatches,
    string? SentenceMachineTranslationPolish,
    string? SentenceMachineTranslationEnglish,
    string? SentenceHumanTranslationPolish,
    string? SentenceHumanTranslationEnglish,
    string? PolishTranslationOfTheWordNominative)
{
    private int? _qualityPenaltyCache;
    public int QualityPenalty
    {
        get
        {
            _qualityPenaltyCache ??= ComputeQualityPenalty();
            return _qualityPenaltyCache.Value;
        }
    }

    /// <summary>
    /// Heuristic quality score.
    /// The lower the number, the more likely this example will be useful in an Anki Card.
    /// For example, I believe that short sentences (3-5 words) are much better in the role of examples to teach basic words than
    /// long sentences.
    /// </summary>
    private int ComputeQualityPenalty()
    {
        var lengthPenalty = Sentence.Words.Count switch
        {
            1 => 2,
            2 => 2, // sometimes good (до речі, Не знаю), but more often too little context (Не ти, будь ласка)
            3 => 1,
            4 => 0,
            5 => 0, // 5 is perfect example
            6 => 1, // 6 has noticeably higher cognitive load already
            7 => 2,
            8 => 3,
            9 => 4,
            _ => 5
        };

        //var missingEnTranslationPenalty = SentenceHumanTranslationEnglish is null ? 1 : 0;
        var missingPlTranslationPenalty = SentenceHumanTranslationPolish is null ? 1 : 0; // reduced from 2 to 1, I don't care that much as human translations are not literal and need investigation anyway
        var difficultContextPenalty = NumWordsWithLessUsages * 1;

        // coz no, `— Гаррі, Гаррі, Гаррі!" is not the best example possible.
        // invariant case ignored for startup perf
        var distinctWordsCount = Sentence.Words.Distinct(/*StringComparer.InvariantCultureIgnoreCase*/).Count();
        var repeatedWordsPenalty = (Sentence.Words.Count - distinctWordsCount) * 1;

        var poorAudioQualityPenalty = TranscriptMatches
            .Any(x => x.PathToAudioFile.Contains("hp_01") ||
                      x.PathToAudioFile.Contains("hp_02") ||
                      x.PathToAudioFile.Contains("hp_03") ||
                      x.PathToAudioFile.Contains("hp_07"))
            ? 0 : 1;

        // prefer examples where human-translated sentence contains the word in the exact form.
        // good: "*He* was there" -> "*On* tam był"  
        // bad : "*He* was there" -> "Był tam."
        var translatedWordDoesNotOccurInTargetLanguageExamplePenalty =
            (SentenceHumanTranslationPolish?.Contains(PolishTranslationOfTheWordNominative) ?? SentenceMachineTranslationPolish?.Contains(PolishTranslationOfTheWordNominative) ?? false)
            ? 0
            : 1;

        return lengthPenalty +
               missingPlTranslationPenalty
               //+ missingEnTranslationPenalty
               + difficultContextPenalty
               + repeatedWordsPenalty
               + poorAudioQualityPenalty
               + translatedWordDoesNotOccurInTargetLanguageExamplePenalty
               ;
    }


    public int NumWordsWithLessUsages { get; set; }
    public string Word { get; init; } = word;
    public Sentence Sentence { get; init; } = Sentence;
    public List<SentenceWithSound> TranscriptMatches { get; init; } = TranscriptMatches;
    public string? SentenceMachineTranslationPolish { get; init; } = SentenceMachineTranslationPolish;
    public string? SentenceMachineTranslationEnglish { get; init; } = SentenceMachineTranslationEnglish;
    public string? SentenceHumanTranslationPolish { get; init; } = SentenceHumanTranslationPolish;
    public string? SentenceHumanTranslationEnglish { get; init; } = SentenceHumanTranslationEnglish;
    public string? PolishTranslationOfTheWordNominative { get; init; } = PolishTranslationOfTheWordNominative;
}
