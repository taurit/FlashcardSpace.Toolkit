namespace AnkiCardValidator.Converters;

public class PenaltyToColorConverter : OneWayConverter
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int penalty)
        {
            return penalty switch
            {
                0 => Brushes.DarkSeaGreen,
                <= 1 => Brushes.Yellow,
                <= 2 => Brushes.Orange,
                _ => Brushes.IndianRed
            };
        }

        return Brushes.Transparent;
    }
}
