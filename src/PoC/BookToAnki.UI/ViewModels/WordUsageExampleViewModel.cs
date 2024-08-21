using BookToAnki.Models;
using BookToAnki.NotePropertiesDatabase;
using CoreLibrary.Interfaces;
using PropertyChanged;
using System.Collections.Generic;

namespace BookToAnki.UI.ViewModels;

[AddINotifyPropertyChangedInterface]
public class WordUsageExampleViewModel
{
    public WordUsageExampleViewModel(WordUsageExample wordUsageExample,
        string Word,
        Sentence Sentence,
        List<SentenceWithSound> TranscriptMatches,
        int QualityPenalty,
        string? SentenceHumanTranslationPolish,
        string? SentenceHumanTranslationEnglish,
        Rating? Rating
        )
    {
        WordUsageExample = wordUsageExample;
        this.Word = Word;
        this.Sentence = Sentence;
        this.TranscriptMatches = TranscriptMatches;
        this.QualityPenalty = QualityPenalty;
        this.SentenceHumanTranslationPolish = SentenceHumanTranslationPolish;
        this.SentenceHumanTranslationEnglish = SentenceHumanTranslationEnglish;
        this.Rating = Rating;
    }
    public string SentenceText => Sentence.Text;

    public WordUsageExample WordUsageExample { get; }
    public string Word { get; init; }
    public Sentence Sentence { get; init; }
    private List<SentenceWithSound> TranscriptMatches { get; init; }
    public int QualityPenalty { get; init; }
    public int TranscriptMatchesCount => TranscriptMatches.Count;
    public string? SentenceHumanTranslationPolish { get; init; }
    public string? SentenceHumanTranslationEnglish { get; init; }
    public Rating? Rating { get; }
}
