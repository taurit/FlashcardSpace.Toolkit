using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace AnkiCardValidator.Converters;

public class PenaltyToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // Check if the value is an integer
        if (value is int penalty)
        {
            if (penalty == 0)
            {
                return Brushes.DarkSeaGreen;
            }
            else if (penalty <= 1)
            {
                return Brushes.Yellow;
            }
            else if (penalty <= 2)
            {
                return Brushes.Orange;
            }
            else
            {
                return Brushes.IndianRed;
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
