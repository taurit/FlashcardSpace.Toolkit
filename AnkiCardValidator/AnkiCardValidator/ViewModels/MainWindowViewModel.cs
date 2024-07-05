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
public sealed class FlashcardViewModel(AnkiNote note, string frontSide, string backSide, CefrClassification cefrClassification, string qualityIssues, string dialect, ObservableCollection<Meaning> meanings, bool? isFlashcardWorthIncludingForA2LevelStudents, string isFlashcardWorthIncludingJustification, string rawResponseFromChatGptApi)
{
    public AnkiNote Note { get; } = note;
    public string FrontSide { get; } = frontSide;
    public string BackSide { get; } = backSide;

    public CefrClassification CefrClassification { get; set; } = cefrClassification;
    public string Dialect { get; set; } = dialect;
    public ObservableCollection<Meaning> Meanings { get; set; } = meanings;
    public bool? IsFlashcardWorthIncludingForA2LevelStudents { get; set; } = isFlashcardWorthIncludingForA2LevelStudents;
    public string IsFlashcardWorthIncludingJustification { get; set; } = isFlashcardWorthIncludingJustification;
    public string QualityIssues { get; set; } = qualityIssues;
    public string RawResponseFromChatGptApi { get; set; } = rawResponseFromChatGptApi;
}
