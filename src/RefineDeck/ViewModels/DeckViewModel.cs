using CoreLibrary.Interfaces;
using CoreLibrary.Models;
using PropertyChanged;
using System.Collections.ObjectModel;

namespace RefineDeck.ViewModels;

[AddINotifyPropertyChangedInterface]
public class DeckViewModel
{
    public DeckPath DeckPath { get; set; }
    public ObservableCollection<ReviewedCardViewModel> Flashcards { get; set; } = [];
    public string? MediaFileNamePrefix { get; set; }

    public SupportedLanguage SourceLanguage { get; set; }
    public SupportedLanguage TargetLanguage { get; set; }
}
