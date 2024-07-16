using AnkiCardValidator.Models;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace AnkiCardValidator.Converters;

public class PenaltyToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
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

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class NumMeaningsPenaltyToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
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

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class NumDuplicatesPenaltyToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int penalty)
        {
            return penalty switch
            {
                <= 0 => Brushes.DarkSeaGreen,
                <= 1 => Brushes.Orange,
                _ => Brushes.IndianRed
            };
        }

        return Brushes.Transparent;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class NumTermsPenaltyToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
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

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class FrequencyPenaltyToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
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

        return Brushes.Orange;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}


public class CefrPenaltyToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
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

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class QualityIssuesToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
            return Brushes.Yellow;
        if (value is string qualityIssues && String.IsNullOrWhiteSpace(qualityIssues))
        {
            return Brushes.DarkSeaGreen;
        }

        return Brushes.IndianRed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
