using PropertyChanged;
using System.Windows.Media;

namespace BookToAnki.UI.ViewModels;

[AddINotifyPropertyChangedInterface]
public class ImageInfo
{
    public ImageInfo(string fileName, bool isSelected)
    {
        FileName = fileName;
        IsSelected = isSelected;
    }

    public string FileName { get; }
    public bool IsSelected { get; set; }

    public SolidColorBrush BorderBrush => IsSelected ? Brushes.DeepSkyBlue : Brushes.LightGray;
}
