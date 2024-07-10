using AnkiCardValidator.Models;
using AnkiCardValidator.Utilities;
using PropertyChanged;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace AnkiCardValidator.ViewModels;

[AddINotifyPropertyChangedInterface]
public class MainWindowViewModel
{
    public ObservableCollection<FlashcardViewModel> Flashcards { get; set; } = [];
    public FlashcardViewModel? SelectedFlashcard { get; set; } = null;
}

[AddINotifyPropertyChangedInterface]
public record MeaningViewModel(string EnglishEquivalent, string Definition);

[AddINotifyPropertyChangedInterface]
[DebuggerDisplay("{FrontSide} -> {BackSide}")]
public sealed class FlashcardViewModel(
    // raw source data
    AnkiNote note,

    // derived from source data locally
    List<AnkiNote> duplicatesFront,
    List<AnkiNote> duplicatesBack,
    int? frequencyPositionFrontSide,
    int? frequencyPositionBackSide,
    int numDefinitionsOnFrontSide,
    int numDefinitionsOnBackSide,

    // derived from source data using ChatGPT
    CefrClassification cefrLevel,
    string? qualityIssues,
    string? rawResponseFromChatGptApi
)
{
    // reference to the evaluated note
    public AnkiNote Note { get; } = note;

    // quality signals calculated locally
    public int? FrequencyPositionFrontSide { get; } = frequencyPositionFrontSide;
    public int? FrequencyPositionBackSide { get; } = frequencyPositionBackSide;

    public int NumDefinitionsOnFrontSide { get; } = numDefinitionsOnFrontSide;
    public int NumDefinitionsOnBackSide { get; } = numDefinitionsOnBackSide;

    public ObservableCollection<AnkiNote> DuplicatesOfFrontSide { get; } = new(duplicatesFront);
    public ObservableCollection<AnkiNote> DuplicatesOfBackSide { get; } = new(duplicatesBack);

    // data received from ChatGPT
    public string? RawResponseFromChatGptApi { get; set; } = rawResponseFromChatGptApi;

    public CefrClassification CefrLevel { get; set; } = cefrLevel;
    public string? QualityIssues { get; set; } = qualityIssues;
    public ObservableCollection<Meaning> Meanings { get; init; } = [];

    // data derived from ChatGPT response
    [DependsOn(nameof(QualityIssues))] private bool HasQualityIssues => !String.IsNullOrWhiteSpace(QualityIssues);


    [DependsOn(nameof(CefrLevel), nameof(HasQualityIssues), nameof(Meanings), nameof(NumDefinitionsOnFrontSide), nameof(NumDefinitionsOnBackSide))]
    public int Penalty =>
        // missing information about CEFR level
        (this.CefrLevel == CefrClassification.Unknown ? 1 : 0) +

        // words with CEFR level C1 and higher should be prioritized down until I learn basics
        (this.CefrLevel >= CefrClassification.C1 ? 1 : 0) +

        // words with CEFR level C2 should be prioritized down even more than B2
        (this.CefrLevel >= CefrClassification.C2 ? 1 : 0) +

        // the more individual meanings word has, the more confusing learning it with flashcards might be
        (Meanings.Count > 0 ? Meanings.Count - 1 : 0) +

        // ChatGPT raised at least one quality issue
        (HasQualityIssues ? 1 : 0) +

        // word appears to have duplicates in the deck (front side)
        DuplicatesOfFrontSide.Count +

        // word appears to have duplicates in the deck (back side)
        DuplicatesOfBackSide.Count +

        // number of terms on the side of the flashcard. For example, if the front contains text 'mnich, zakonnik', this will be 2
        // (the ideal number is 1)
        (NumDefinitionsOnFrontSide - 1) +
        (NumDefinitionsOnBackSide - 1) +

        // no frequency data - this can be false negative, if term is a sentence, or HTML tags weren't sanitized.
        // I can improve false alarms with heuristics
        (FrequencyPositionFrontSide.HasValue ? 0 : 1) +
        (FrequencyPositionBackSide.HasValue ? 0 : 1) +

        // frequency data exists and suggests that Spanish word is used very infrequently
        (FrequencyPositionFrontSide.HasValue ? CalculateFrequencyPenalty(FrequencyPositionFrontSide.Value) : 0) +

        // same for polish side
        (FrequencyPositionBackSide.HasValue ? CalculateFrequencyPenalty(FrequencyPositionBackSide.Value) : 0)
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
