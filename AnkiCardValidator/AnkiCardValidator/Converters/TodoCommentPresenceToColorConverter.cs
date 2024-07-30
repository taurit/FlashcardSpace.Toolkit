namespace AnkiCardValidator.Converters;

public class TodoCommentPresenceToColorConverter : OneWayConverter
{
    public override object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool todoValueIsPresent)
        {
            return todoValueIsPresent ? Brushes.Red : Brushes.DarkSeaGreen;
        }

        return Brushes.Transparent;
    }
}
