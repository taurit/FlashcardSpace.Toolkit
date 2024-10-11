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

    [DependsOn(nameof(Deck), nameof(SelectedFlashcard))]
    public int NumPending => Deck.Flashcards.Count(flashcard => flashcard.ApprovalStatus == ApprovalStatus.NotReviewedYet);

    [DependsOn(nameof(Deck), nameof(SelectedFlashcard))]
    public int NumApproved => Deck.Flashcards.Count(flashcard => flashcard.ApprovalStatus == ApprovalStatus.Approved);

    [DependsOn(nameof(Deck), nameof(SelectedFlashcard))]
    public int NumRejected => Deck.Flashcards.Count(flashcard => flashcard.ApprovalStatus == ApprovalStatus.Rejected);

    [DependsOn(nameof(Deck), nameof(SelectedFlashcard))]
    public int NumWarnings => Deck.Flashcards.Count(flashcard => flashcard.HasPendingQaSuggestions);

    [DependsOn(nameof(Deck), nameof(SelectedFlashcard))]
    public int NumWarningsSecondOpinion => Deck.Flashcards.Count(flashcard => flashcard.HasPendingQaSuggestionsSecondOpinion);

    public bool PerformingQualityAnalysis { get; set; }
}
