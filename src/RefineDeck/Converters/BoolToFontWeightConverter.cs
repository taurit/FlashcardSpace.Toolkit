using System.Globalization;
using System.Windows;

namespace RefineDeck.Converters;
internal class BoolToFontWeightConverter : OneWayConverter
{
    public override object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue && boolValue)
        {
            return FontWeights.Bold;
        }

        return FontWeights.Normal;
    }
}
