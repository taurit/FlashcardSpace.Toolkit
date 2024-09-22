using System.Windows;
using System.Windows.Media;

namespace RefineDeck.Utils;

internal static class VisualTreeHelpers
{
    internal static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T typedChild)
            {
                return typedChild;
            }

            var result = FindVisualChild<T>(child);
            if (result != null)
                return result;
        }
        return null;
    }
}
