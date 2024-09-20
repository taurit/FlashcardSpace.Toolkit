using PropertyChanged;

namespace RefineDeck.ViewModels;

[AddINotifyPropertyChangedInterface]
public class MainWindowViewModel
{
    public DeckViewModel Deck { get; set; }

    //public CardViewModel? SelectedCard { get; set; } = null;

}
