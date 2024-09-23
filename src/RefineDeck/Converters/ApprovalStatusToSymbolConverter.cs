using CoreLibrary.Models;
using System.Globalization;

namespace RefineDeck.Converters;

public class ApprovalStatusToSymbolConverter : OneWayConverter
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ApprovalStatus status)
        {
            return status switch
            {
                // empty checkbox
                ApprovalStatus.NotReviewedYet => "",
                ApprovalStatus.Approved => "☑",
                ApprovalStatus.Rejected => "❌",

                // unexpected
                _ => "❓",
            };
        }

        return "❓";
    }
}
