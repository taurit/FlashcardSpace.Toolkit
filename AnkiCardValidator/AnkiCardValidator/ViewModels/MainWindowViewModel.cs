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
public sealed class FlashcardViewModel(AnkiNote note, string frontSide, string backSide, CefrClassification cefrClassification, List<Warning> warnings, string comments)
{
    public AnkiNote Note { get; } = note;
    public string FrontSide { get; } = frontSide;
    public string BackSide { get; } = backSide;

    public CefrClassification CefrClassification { get; set; } = cefrClassification;
    public string Comments { get; set; } = comments;
    public List<Warning> Warnings { get; } = warnings;

    public int? QualityPenalty => Warnings.Any() ? Warnings.Sum(x => x.Penalty) : null;
}

[AddINotifyPropertyChangedInterface]
public sealed record Warning(string Message, int Penalty);
