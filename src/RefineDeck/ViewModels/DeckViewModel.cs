using PropertyChanged;
using System.Collections.ObjectModel;

namespace RefineDeck.ViewModels;

[AddINotifyPropertyChangedInterface]
public class DeckViewModel
{
    public string? DeckFolderPath { get; set; }
    public ObservableCollection<ReviewedCardViewModel> Flashcards { get; set; } = [];
}
