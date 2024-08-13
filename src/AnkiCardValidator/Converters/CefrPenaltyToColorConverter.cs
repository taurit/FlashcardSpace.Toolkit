using AnkiCardValidator.Models;

namespace AnkiCardValidator.Converters;

public class CefrPenaltyToColorConverter : OneWayConverter
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is CefrClassification classification)
        {
            return classification switch
            {
                CefrClassification.Unknown => Brushes.Yellow,
                CefrClassification.A1 => Brushes.DarkSeaGreen,
                CefrClassification.A2 => Brushes.DarkSeaGreen,
                CefrClassification.B1 => Brushes.DarkSeaGreen,
                CefrClassification.B2 => Brushes.Yellow,
                CefrClassification.C1 => Brushes.Orange,
                CefrClassification.C2 => Brushes.IndianRed,
                _ => Brushes.IndianRed
            };
        }

        return Brushes.Transparent;


    }
}
