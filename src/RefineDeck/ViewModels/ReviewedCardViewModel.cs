using CoreLibrary.Models;
using PropertyChanged;
using RefineDeck.Utils;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;

namespace RefineDeck.ViewModels;

[AddINotifyPropertyChangedInterface]
public class ReviewedCardViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    public FlashcardNote OriginalFlashcard { get; set; }

    public bool IsAnythingOverridden => IsTermOverridden || IsTermTranslationOverridden || IsSentenceExampleOverridden
                                        || IsSentenceExampleTranslationOverridden || IsRemarksFieldOverridden
                                        || IsSelectedImageIndexOverridden || IsApprovalStatusOverridden
                                        || IsQaSuggestionsOverridden
                                        ;

    public string Term { get; set; }
    public bool IsTermOverridden => Term != OriginalFlashcard.Term;

    public string TermAudio { get; set; }
    public bool IsTermAudioOverridden => TermAudio != OriginalFlashcard.TermAudio;

    public string TermTranslation { get; set; }
    public bool IsTermTranslationOverridden => TermTranslation != OriginalFlashcard.TermTranslation;

    public string TermTranslationAudio { get; set; }
    public bool IsTermTranslationAudioOverridden => TermTranslationAudio != OriginalFlashcard.TermTranslationAudio;

    public string SentenceExample { get; set; }
    public bool IsSentenceExampleOverridden => SentenceExample != OriginalFlashcard.Context;

    public string SentenceExampleAudio { get; set; }
    public bool IsSentenceExampleAudioOverridden => SentenceExampleAudio != OriginalFlashcard.ContextAudio;

    public string SentenceExampleTranslation { get; set; }
    public bool IsSentenceExampleTranslationOverridden => SentenceExampleTranslation != OriginalFlashcard.ContextTranslation;

    public string Remarks { get; set; }
    public bool IsRemarksFieldOverridden => Remarks != OriginalFlashcard.Remarks;

    public string QaSuggestions { get; set; }
    public bool IsQaSuggestionsOverridden => QaSuggestions != OriginalFlashcard.QaSuggestions;

    public PlainTextAndJsonPart? QaSuggestionsSecondOpinion { get; set; }

    public bool HasPendingQaSuggestions => !String.IsNullOrWhiteSpace(QaSuggestions);
    public bool HasPendingQaSuggestionsSecondOpinion => QaSuggestionsSecondOpinion is not null && !QaSuggestionsSecondOpinion.PlainText.StartsWith("OK");

    public bool HasPendingQaSuggestionsSecondOpinionJson => QaSuggestionsSecondOpinion is not null && QaSuggestionsSecondOpinion.Suggestion is not null;
    public bool HasPendingQaSuggestionsSecondOpinionJsonTerm => QaSuggestionsSecondOpinion is not null &&
                                                                QaSuggestionsSecondOpinion.Suggestion is not null &&
                                                                QaSuggestionsSecondOpinion.Suggestion.FrontSide_Question != Term;
    public bool HasPendingQaSuggestionsSecondOpinionJsonTermTranslation => QaSuggestionsSecondOpinion is not null &&
                                                                            QaSuggestionsSecondOpinion.Suggestion is not null &&
                                                                            QaSuggestionsSecondOpinion.Suggestion.BackSide_Answer != TermTranslation;
    public bool HasPendingQaSuggestionsSecondOpinionJsonSentenceExample => QaSuggestionsSecondOpinion is not null &&
                                                                           QaSuggestionsSecondOpinion.Suggestion is not null &&
                                                                           QaSuggestionsSecondOpinion.Suggestion.SentenceExample != SentenceExample;
    public bool HasPendingQaSuggestionsSecondOpinionJsonSentenceExampleTranslation => QaSuggestionsSecondOpinion is not null &&
                                                           QaSuggestionsSecondOpinion.Suggestion is not null &&
                                                           QaSuggestionsSecondOpinion.Suggestion.SentenceExampleTranslation != SentenceExampleTranslation;
    public bool HasPendingQaSuggestionsSecondOpinionJsonRemarks => QaSuggestionsSecondOpinion is not null &&
                                                                    QaSuggestionsSecondOpinion.Suggestion is not null &&
                                                                    QaSuggestionsSecondOpinion.Suggestion.RemarksFromTeacherToStudent != Remarks;



    public bool SecondOpinionConfirmedOk => QaSuggestionsSecondOpinion is not null && QaSuggestionsSecondOpinion.PlainText.StartsWith("OK");
    public bool HasAnyPendingQaSuggestions => HasPendingQaSuggestions || HasPendingQaSuggestionsSecondOpinion;

    public ObservableCollection<ImageCandidate> ImageCandidates { get; set; }

    // Convention: image representing "no image" is last in the list
    public int? SelectedImageIndex { get; set; }
    public bool IsSelectedImageIndexOverridden => SelectedImageIndex != OriginalFlashcard.SelectedImageIndex;

    public string? SelectedImage
    {
        get
        {
            if (!SelectedImageIndex.HasValue) return null;

            var absolutePath = ImageCandidates[SelectedImageIndex.Value].AbsolutePath;

            // convention: see if there is an improved image variant available
            var improvedVariantPath = absolutePath.Replace(".jpg", ".improved.jpg");
            if (File.Exists(improvedVariantPath))
            {
                return improvedVariantPath;
            }

            return absolutePath;
        }
    }
    public bool IsPlaceholderImageSelected => (!SelectedImageIndex.HasValue) || ImageCandidates[SelectedImageIndex.Value].RelativePath is null;

    public ApprovalStatus ApprovalStatus { get; set; }
    public bool IsApprovalStatusOverridden => ApprovalStatus != OriginalFlashcard.ApprovalStatus;


}
