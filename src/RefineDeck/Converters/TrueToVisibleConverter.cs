using System.Globalization;
using System.Windows;

namespace RefineDeck.Converters;
internal class TrueToVisibleConverter : OneWayConverter
{
    public override object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool hasQaRemarks)
        {
            return hasQaRemarks ? Visibility.Visible : Visibility.Collapsed;
        }

        return Visibility.Collapsed;

    }
}
