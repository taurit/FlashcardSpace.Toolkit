using PropertyChanged;

namespace RefineDeck.ViewModels;

[AddINotifyPropertyChangedInterface]
public class ReviewedCardViewModel
{
    public string Term { get; set; }
    public string TermTranslation { get; set; }

    public ApprovalStatus ApprovalStatus { get; set; }
}
