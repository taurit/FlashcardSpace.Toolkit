using AnkiCardValidator.Models;
using AnkiCardValidator.Utilities;
using PropertyChanged;
using System.Collections.ObjectModel;

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
public sealed class FlashcardViewModel(
    // raw source data
    AnkiNote note,
    string frontSide,
    string backSide,

    // derived from source data locally
    HashSet<string> tags,
    List<AnkiNote> duplicatesFront,
    List<AnkiNote> duplicatesBack,
    int? frequencyPositionFrontSide,
    int? frequencyPositionBackSide,

    // derived from source data using ChatGPT
    CefrClassification cefrLevel,
    string? qualityIssues,
    string? dialect,
    string? rawResponseFromChatGptApi
)
{
    // reference to the evaluated note
    public AnkiNote Note { get; } = note;

    // data loaded from Anki
    public string FrontSide { get; } = frontSide;
    public string BackSide { get; } = backSide;
    public HashSet<string> Tags { get; } = tags;

    [DependsOn(nameof(Tags))] public string TagsSerialized => String.Join(",", Tags);

    // quality signals calculated locally
    public int? FrequencyPositionFrontSide { get; } = frequencyPositionFrontSide;
    public int? FrequencyPositionBackSide { get; } = frequencyPositionBackSide;

    public ObservableCollection<AnkiNote> DuplicatesOfFrontSide { get; } = new(duplicatesFront);
    public ObservableCollection<AnkiNote> DuplicatesOfBackSide { get; } = new(duplicatesBack);

    // data received from ChatGPT
    public string? RawResponseFromChatGptApi { get; set; } = rawResponseFromChatGptApi;

    public CefrClassification CefrLevel { get; set; } = cefrLevel;
    public string? Dialect { get; set; } = dialect;
    public string? QualityIssues { get; set; } = qualityIssues;
    public ObservableCollection<Meaning> Meanings { get; init; } = [];

    // data derived from ChatGPT response
    [DependsOn(nameof(QualityIssues))] private bool HasQualityIssues => !String.IsNullOrWhiteSpace(QualityIssues);


    [DependsOn(nameof(CefrLevel), nameof(HasQualityIssues), nameof(Meanings))]
    public int Penalty =>
        // missing information about CEFR level
        (this.CefrLevel == CefrClassification.Unknown ? 1 : 0) +

        // words with CEFR level B2 and higher should be prioritized down until I learn basics
        (this.CefrLevel >= CefrClassification.B2 ? 1 : 0) +

        // words with CEFR level C1 and higher should be prioritized down even more than B2
        (this.CefrLevel >= CefrClassification.C1 ? 1 : 0) +

        // the more individual meanings word has, the more confusing learning it with flashcards might be
        Meanings.Count +

        // ChatGPT raised at least one quality issue
        (HasQualityIssues ? 1 : 0) +

        // word appears to have duplicates in the deck (front side)
        DuplicatesOfFrontSide.Count +

        // word appears to have duplicates in the deck (back side)
        DuplicatesOfBackSide.Count

        ;
}
