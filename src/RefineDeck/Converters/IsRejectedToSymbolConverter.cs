using System.Globalization;

namespace RefineDeck.Converters;

public class IsRejectedToSymbolConverter : OneWayConverter
{
    public override object Convert(object? value, Type targetType, object parameter, CultureInfo culture) => value is true ? "❌" : "";
}
