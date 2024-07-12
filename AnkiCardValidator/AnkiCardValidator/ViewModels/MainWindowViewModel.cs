using AnkiCardValidator.Models;
using AnkiCardValidator.Utilities;
using PropertyChanged;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace AnkiCardValidator.ViewModels;

[AddINotifyPropertyChangedInterface]
public class MainWindowViewModel
{
    public ObservableCollection<CardViewModel> Flashcards { get; set; } = [];
    public CardViewModel? SelectedCard { get; set; } = null;
}

[AddINotifyPropertyChangedInterface]
public record MeaningViewModel(string EnglishEquivalent, string Definition);

[AddINotifyPropertyChangedInterface]
[DebuggerDisplay("{FrontSide} -> {BackSide}")]
public sealed class CardViewModel(
    // raw source data
    AnkiNote note,

    // derived from source data locally
    List<AnkiNote> duplicatesQuestion,
    List<AnkiNote> duplicatesAnswer,
    int? frequencyPositionQuestion,
    int? frequencyPositionAnswer,
    int numDefinitionsForQuestion,
    int numDefinitionsForAnswer,

    // derived from source data using ChatGPT
    CefrClassification cefrLevelQuestion,
    string? qualityIssues,
    string? rawResponseFromChatGptApi
)
{
    // reference to the evaluated note
    public AnkiNote Note { get; } = note;

    // quality signals calculated locally
    public int? FrequencyPositionQuestion { get; } = frequencyPositionQuestion;
    public int? FrequencyPositionAnswer { get; } = frequencyPositionAnswer;

    public int NumDefinitionsForQuestion { get; } = numDefinitionsForQuestion;
    public int NumDefinitionsForAnswer { get; } = numDefinitionsForAnswer;

    public ObservableCollection<AnkiNote> DuplicatesOfQuestion { get; } = new(duplicatesQuestion);
    public ObservableCollection<AnkiNote> DuplicatesOfAnswer { get; } = new(duplicatesAnswer);

    // data received from ChatGPT
    public string? RawResponseFromChatGptApi { get; set; } = rawResponseFromChatGptApi;

    public CefrClassification CefrLevelQuestion { get; set; } = cefrLevelQuestion;
    public string? QualityIssues { get; set; } = qualityIssues;
    public ObservableCollection<Meaning> Meanings { get; init; } = [];

    // data derived from ChatGPT response
    [DependsOn(nameof(QualityIssues))] private bool HasQualityIssues => !String.IsNullOrWhiteSpace(QualityIssues);

    [DependsOn(nameof(CefrLevelQuestion), nameof(HasQualityIssues), nameof(Meanings), nameof(NumDefinitionsForQuestion), nameof(NumDefinitionsForAnswer))]
    public int Penalty =>
        // missing information about CEFR level
        (this.CefrLevelQuestion == CefrClassification.Unknown ? 1 : 0) +

        // words with CEFR level C1 and higher should be prioritized down until I learn basics
        (this.CefrLevelQuestion >= CefrClassification.C1 ? 1 : 0) +

        // words with CEFR level C2 should be prioritized down even more than B2
        (this.CefrLevelQuestion >= CefrClassification.C2 ? 1 : 0) +

        // the more individual meanings word has, the more confusing learning it with flashcards might be
        (Meanings.Count > 0 ? Meanings.Count - 1 : 0) +

        // ChatGPT raised at least one quality issue
        (HasQualityIssues ? 1 : 0) +

        // word appears to have duplicates in the deck (front side)
        DuplicatesOfQuestion.Count +

        // word appears to have duplicates in the deck (back side)
        DuplicatesOfAnswer.Count +

        // number of terms on the side of the flashcard. For example, if the front contains text 'mnich, zakonnik', this will be 2
        // (the ideal number is 1)
        (NumDefinitionsForQuestion - 1) +
        (NumDefinitionsForAnswer - 1) +

        // no frequency data - this can be false negative, if term is a sentence, or HTML tags weren't sanitized.
        // I can improve false alarms with heuristics
        (FrequencyPositionQuestion.HasValue ? 0 : 1) +
        (FrequencyPositionAnswer.HasValue ? 0 : 1) +

        // frequency data exists and suggests that Spanish word is used very infrequently
        (FrequencyPositionQuestion.HasValue ? CalculateFrequencyPenalty(FrequencyPositionQuestion.Value) : 0) +

        // same for polish side
        (FrequencyPositionAnswer.HasValue ? CalculateFrequencyPenalty(FrequencyPositionAnswer.Value) : 0)
        ;

    private int CalculateFrequencyPenalty(int position) => position switch
    {
        < 10000 => 0,
        < 20000 => 1,
        < 30000 => 2,
        < 40000 => 3,
        _ => 4
    };
}
