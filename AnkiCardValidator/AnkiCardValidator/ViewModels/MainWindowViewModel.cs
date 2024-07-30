using PropertyChanged;
using System.Collections.ObjectModel;

namespace AnkiCardValidator.ViewModels;

[AddINotifyPropertyChangedInterface]
public class MainWindowViewModel
{
    public ObservableCollection<CardViewModel> Flashcards { get; set; } = [];
    public CardViewModel? SelectedCard { get; set; } = null;
    public string? StatusMessage { get; set; }
}
