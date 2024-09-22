using CoreLibrary.Models;
using PropertyChanged;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace RefineDeck.ViewModels;

[AddINotifyPropertyChangedInterface]
public class ReviewedCardViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    public string Term { get; set; }
    public string TermTranslation { get; set; }

    public string SentenceExample { get; set; }
    public string SentenceExampleTranslation { get; set; }

    public ObservableCollection<string> ImageCandidates { get; set; }
    public string? SelectedImageFullPath { get; set; }

    [DoNotNotify] // to avoid re-rendering preview twice. This always changes when SelectedImageFullPath changes.
    public string? SelectedImageRelativePath
    {
        get
        {
            if (String.IsNullOrWhiteSpace(SelectedImageFullPath)) return null;
            // keep only the part relative to flashcards.json (Deck's root folder)
            var parts = SelectedImageFullPath.Split(['\\', '/']);
            var relativePath = string.Join('/', parts[^2..]);
            return relativePath;
        }
    }

    public ApprovalStatus ApprovalStatus { get; set; }
    public ObservableCollection<Warning> Warnings { get; set; } = new();

    public FlashcardNote OriginalFlashcard { get; set; }
}
