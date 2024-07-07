using AnkiCardValidator.Models;
using AnkiCardValidator.Utilities;
using PropertyChanged;
using System.Collections.ObjectModel;

namespace AnkiCardValidator.ViewModels;

[AddINotifyPropertyChangedInterface]
public class MainWindowViewModel
{
    public ObservableCollection<FlashcardViewModel> Flashcards { get; set; } =
    [
        //new FlashcardViewModel(1, "Front side 1", "Back side 1", CefrClassification.Unknown, new List<Warning>() { new Warning("123", 1)} ),
        //new FlashcardViewModel(2, "Front side 2", "Back side 2", CefrClassification.Unknown, new List<Warning>(){ new Warning("1234", 0)}),
        //new FlashcardViewModel(3, "Front side 3", "Back side 3", CefrClassification.Unknown, new List<Warning>(){ new Warning("12356", 3)}),
    ];

    public FlashcardViewModel? SelectedFlashcard { get; set; } = null;
}

[AddINotifyPropertyChangedInterface]
public record MeaningViewModel(string EnglishEquivalent, string Definition);

[AddINotifyPropertyChangedInterface]
public sealed class FlashcardViewModel(
    AnkiNote note,
    string frontSide,
    string backSide,
    HashSet<string> tags,
    int? frequencyPosition,
    CefrClassification cefrLevel,
    string? qualityIssues,
    string? dialect,
    string? rawResponseFromChatGptApi)
{
    // reference to the evaluated note
    public AnkiNote Note { get; } = note;

    // data loaded from Anki
    public string FrontSide { get; } = frontSide;
    public string BackSide { get; } = backSide;
    public int? FrequencyPosition { get; } = frequencyPosition;
    public HashSet<string> Tags { get; } = tags;

    [DependsOn(nameof(Tags))] public string TagsSerialized => String.Join(",", Tags);

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
