using BookToAnki.NotePropertiesDatabase;
using PropertyChanged;
using System.Windows.Media;

namespace BookToAnki.UI.ViewModels;

[AddINotifyPropertyChangedInterface]
public class CardRatingFlowViewModel
{
    public Rating? CurrentRating { get; set; }

    [DependsOn(nameof(CurrentRating))]
    public Brush? RejectButtonBorder => CurrentRating == Rating.Rejected ? new SolidColorBrush(Colors.Black) : null;

    [DependsOn(nameof(CurrentRating))]
    public Brush? AverageButtonBorder => CurrentRating == Rating.AcceptableForPragmatics ? new SolidColorBrush(Colors.Black) : null;

    [DependsOn(nameof(CurrentRating))]
    public Brush? AcceptButtonBorder => CurrentRating == Rating.Premium ? new SolidColorBrush(Colors.Black) : null;

    public int NumExamplesRatedOnFlowStart { get; set; }
    public int CurrentCardIndex { get; set; } // = rated or skipped since the flow started

    [DependsOn(nameof(NumExamplesRatedOnFlowStart), nameof(CurrentCardIndex))]
    public int AlreadyRatedOrSkipped => NumExamplesRatedOnFlowStart + CurrentCardIndex;

    public int TotalNumCardsToRate { get; set; } = 9000; // ~ 3000 * 3 examples


}
