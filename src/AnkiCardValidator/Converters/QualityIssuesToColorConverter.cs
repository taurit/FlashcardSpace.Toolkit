namespace AnkiCardValidator.Converters;


public class QualityIssuesToColorConverter : OneWayConverter
{
    public override object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
            return Brushes.Yellow;
        if (value is string qualityIssues && String.IsNullOrWhiteSpace(qualityIssues))
        {
            return Brushes.DarkSeaGreen;
        }

        return Brushes.IndianRed;
    }
}
