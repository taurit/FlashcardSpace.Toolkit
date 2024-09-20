using RefineDeck.ViewModels;
using System.Globalization;
using System.Windows.Media;

namespace RefineDeck.Converters;
public class ApprovalStatusToColorConverter : OneWayConverter
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ApprovalStatus status)
        {
            return status switch
            {
                ApprovalStatus.NotReviewedYet => Brushes.Transparent,
                ApprovalStatus.Approved => Brushes.DarkCyan,
                ApprovalStatus.Rejected => Brushes.LightCoral,
                ApprovalStatus.RequiresDiscussion => Brushes.LightYellow,

                // unexpected
                _ => Brushes.HotPink,
            };
        }

        return Brushes.Transparent;
    }
}


public class ApprovalStatusToSymbolConverter : OneWayConverter
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ApprovalStatus status)
        {
            return status switch
            {
                // empty checkbox
                ApprovalStatus.NotReviewedYet => "☐",
                ApprovalStatus.Approved => "☑",
                ApprovalStatus.Rejected => "⮽",
                ApprovalStatus.RequiresDiscussion => "💬",

                // unexpected
                _ => "❓",
            };
        }

        return "❓";
    }
}
