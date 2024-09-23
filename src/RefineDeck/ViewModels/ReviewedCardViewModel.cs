using CoreLibrary.Models;
using PropertyChanged;
using RefineDeck.Utils;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace RefineDeck.ViewModels;

[AddINotifyPropertyChangedInterface]
public class ReviewedCardViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    public FlashcardNote OriginalFlashcard { get; set; }

    public bool IsAnythingOverridden => IsTermOverridden || IsTermTranslationOverridden || IsSentenceExampleOverridden || IsSentenceExampleTranslationOverridden || IsTermDefinitionOverridden | IsSelectedImageIndexOverridden;

    public string Term { get; set; }
    public bool IsTermOverridden => this.Term != OriginalFlashcard.Term;

    public string TermTranslation { get; set; }
    public bool IsTermTranslationOverridden => this.TermTranslation != OriginalFlashcard.TermTranslation;

    public string SentenceExample { get; set; }
    public bool IsSentenceExampleOverridden => this.SentenceExample != OriginalFlashcard.Context;

    public string SentenceExampleTranslation { get; set; }
    public bool IsSentenceExampleTranslationOverridden => this.SentenceExampleTranslation != OriginalFlashcard.ContextTranslation;

    public string TermDefinition { get; set; }
    public bool IsTermDefinitionOverridden => this.TermDefinition != OriginalFlashcard.TermDefinition;

    public ObservableCollection<ImageCandidate> ImageCandidates { get; set; }

    // Assumption: image representing "no image" is last in the list
    public int? SelectedImageIndex { get; set; }
    public bool IsSelectedImageIndexOverridden => this.SelectedImageIndex != OriginalFlashcard.SelectedImageIndex;

    public ApprovalStatus ApprovalStatus { get; set; }
    public ObservableCollection<Warning> Warnings { get; set; } = new();

}
