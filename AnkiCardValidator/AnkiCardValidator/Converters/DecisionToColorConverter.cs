using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace AnkiCardValidator.Converters;

public class DecisionToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // Check if the value is an integer
        if (value is bool keepInAnkiDeck)
        {
            if (keepInAnkiDeck)
            {
                return Brushes.DarkSeaGreen;
            }
            else
            {
                return Brushes.IndianRed; // Return a red color for penalties greater than or equal to 10
            }
        }

        // Return a default color if the value is not an integer
        return Brushes.Transparent;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
