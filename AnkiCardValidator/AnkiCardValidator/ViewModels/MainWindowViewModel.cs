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
    public bool? IsFlashcardWorthIncludingInMySet => this.CefrLevel is CefrClassification.Unknown
        ? null
        : (this.CefrLevel is not CefrClassification.Unknown &&
           this.CefrLevel <= CefrClassification.B1 &&
           Meanings.Count <= 2 &&
           !HasQualityIssues);
}
