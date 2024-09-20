using PropertyChanged;
using System.Collections.ObjectModel;

namespace RefineDeck.ViewModels;

[AddINotifyPropertyChangedInterface]
public class ReviewedCardViewModel
{
    public string Term { get; set; }
    public string TermTranslation { get; set; }

    public ApprovalStatus ApprovalStatus { get; set; }
    public ObservableCollection<Warning> Warnings { get; set; } = new();
}

[AddINotifyPropertyChangedInterface]
public class Warning
{
    public string Severity { get; set; }
}
