using PropertyChanged;
using System.Collections.ObjectModel;

namespace AnkiCardValidator.ViewModels;

[AddINotifyPropertyChangedInterface]
public class MainWindowViewModel
{
    public ObservableCollection<FlashcardViewModel> Flashcards { get; set; } =
    [
        new FlashcardViewModel(1, "Front side 1", "Back side 1", CefrClassification.A1),
        new FlashcardViewModel(2, "Front side 2", "Back side 2", CefrClassification.B2),
        new FlashcardViewModel(3, "Front side 3", "Back side 3", CefrClassification.A2),
    ];
}

[AddINotifyPropertyChangedInterface]
public sealed record FlashcardViewModel(int Id, string FrontSide, string BackSide, CefrClassification CefrClassification)
{
    public int QualityPenalty => (int)CefrClassification;
}

public enum CefrClassification
{
    Unknown,
    A1,
    A2,
    B1,
    B2,
    C1,
    C2,
}
