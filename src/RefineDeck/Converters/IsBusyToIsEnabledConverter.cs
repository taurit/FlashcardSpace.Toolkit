using System.Globalization;

namespace RefineDeck.Converters;
public class IsBusyToIsEnabledConverter : OneWayConverter
{
    public override object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isBusy)
        {
            return !isBusy;
        }
        return false;
    }
}
