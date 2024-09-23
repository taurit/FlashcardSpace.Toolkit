using CoreLibrary.Models;
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
                ApprovalStatus.NotReviewedYet => Brushes.Gray,
                ApprovalStatus.Approved => Brushes.DarkSeaGreen,
                ApprovalStatus.Rejected => Brushes.IndianRed,

                // unexpected
                _ => Brushes.RoyalBlue,
            };
        }

        return Brushes.DodgerBlue;
    }
}
