namespace AnkiCardValidator.Converters;

public class FrequencyPenaltyToColorConverter : OneWayConverter
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int penalty)
        {
            return penalty switch
            {
                <= 20000 => Brushes.DarkSeaGreen,
                <= 40000 => Brushes.Orange,
                _ => Brushes.IndianRed
            };
        }

        return Brushes.DarkSeaGreen;
    }
}
