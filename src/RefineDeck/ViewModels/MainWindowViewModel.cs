using CoreLibrary.Models;
using PropertyChanged;
using System.ComponentModel;

namespace RefineDeck.ViewModels;

[AddINotifyPropertyChangedInterface]
public class MainWindowViewModel : INotifyPropertyChanged
{
    // needed explicitly if I want to subscribe to changes from my custom code
    public event PropertyChangedEventHandler PropertyChanged;

    public DeckViewModel Deck { get; set; }

    public ReviewedCardViewModel? SelectedFlashcard { get; set; } = null;

    [DependsOn(nameof(Deck))]
    public int NumPending => Deck.Flashcards.Count(flashcard => flashcard.ApprovalStatus == ApprovalStatus.NotReviewedYet);

    [DependsOn(nameof(Deck))]
    public int NumApproved => Deck.Flashcards.Count(flashcard => flashcard.ApprovalStatus == ApprovalStatus.Approved);

    [DependsOn(nameof(Deck))]
    public int NumRejected => Deck.Flashcards.Count(flashcard => flashcard.ApprovalStatus == ApprovalStatus.Rejected);
}
