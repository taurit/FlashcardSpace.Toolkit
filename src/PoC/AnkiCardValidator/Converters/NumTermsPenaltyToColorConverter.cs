namespace AnkiCardValidator.Converters;

public class NumTermsPenaltyToColorConverter : OneWayConverter
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int penalty)
        {
            return penalty switch
            {
                <= 1 => Brushes.DarkSeaGreen,
                <= 2 => Brushes.Orange,
                _ => Brushes.IndianRed
            };
        }

        return Brushes.Transparent;
    }

}
