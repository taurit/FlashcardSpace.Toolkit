using System.Globalization;
using System.Windows.Data;

namespace AnkiCardValidator.Converters;

public class EmptyStringToCheckboxConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is null) return "Not evaluated yet.";

        if (value is string str)
        {
            if (String.IsNullOrWhiteSpace(str))
            {
                return "\u2705";
            }
            return str;
        }

        return value;
    }

    public object ConvertBack(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
