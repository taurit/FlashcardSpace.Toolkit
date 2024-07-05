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
            // Define your custom logic to convert the penalty value to a color
            // For example, you can set different colors based on the penalty range
            if (penalty == 0)
            {
                return Brushes.Transparent; // currently "0 meanings of a word" means we just haven't evaluated it yet; penalty needs to be redefined
            }
            if (penalty == 1)
            {
                return Brushes.DarkSeaGreen; // Return a green color for penalties between 0 and 5
            }
            else if (penalty < 3)
            {
                return Brushes.LightGoldenrodYellow; // Return a yellow color for penalties between 5 and 10
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
