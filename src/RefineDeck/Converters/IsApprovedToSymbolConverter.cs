using System.Globalization;

namespace RefineDeck.Converters;

public class IsApprovedToSymbolConverter : OneWayConverter
{
    public override object Convert(object? value, Type targetType, object parameter, CultureInfo culture) => value is true ? "☑" : "";
}
