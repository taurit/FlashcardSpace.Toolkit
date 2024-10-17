using System.Globalization;

namespace RefineDeck.Converters;
public class QaStatusToSymbolConverter : OneWayConverter
{
    public override object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is true)
            return String.Intern("⚠️");

        return String.Empty;
    }
}
