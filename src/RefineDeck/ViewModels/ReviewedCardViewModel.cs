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

    public bool IsAnythingOverridden => IsTermOverridden || IsTermTranslationOverridden || IsSentenceExampleOverridden || IsSentenceExampleTranslationOverridden || IsRemarksFieldOverridden | IsSelectedImageIndexOverridden;

    public string Term { get; set; }
    public bool IsTermOverridden => this.Term != OriginalFlashcard.Term;

    public string TermAudio { get; set; }
    public bool IsTermAudioOverridden => this.TermAudio != OriginalFlashcard.TermAudio;

    public string TermTranslation { get; set; }
    public bool IsTermTranslationOverridden => this.TermTranslation != OriginalFlashcard.TermTranslation;

    public string TermTranslationAudio { get; set; }
    public bool IsTermTranslationAudioOverridden => this.TermTranslationAudio != OriginalFlashcard.TermTranslationAudio;

    public string SentenceExample { get; set; }
    public bool IsSentenceExampleOverridden => this.SentenceExample != OriginalFlashcard.Context;

    public string SentenceExampleAudio { get; set; }
    public bool IsSentenceExampleAudioOverridden => this.SentenceExampleAudio != OriginalFlashcard.ContextAudio;

    public string SentenceExampleTranslation { get; set; }
    public bool IsSentenceExampleTranslationOverridden => this.SentenceExampleTranslation != OriginalFlashcard.ContextTranslation;

    public string Remarks { get; set; }
    public bool IsRemarksFieldOverridden => this.Remarks != OriginalFlashcard.Remarks;

    public bool HasQaSuggestions => !String.IsNullOrWhiteSpace(OriginalFlashcard.QaSuggestions);

    public ObservableCollection<ImageCandidate> ImageCandidates { get; set; }

    // Assumption: image representing "no image" is last in the list
    public int? SelectedImageIndex { get; set; }
    public bool IsSelectedImageIndexOverridden => this.SelectedImageIndex != OriginalFlashcard.SelectedImageIndex;

    public ApprovalStatus ApprovalStatus { get; set; }

}
