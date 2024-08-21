using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace SoundEditorControl.Utils;
internal static class ImageExtensions
{
    public static BitmapImage ConvertDrawingImageToWpfImage(this Image gdiImg)
    {
        var memory = new MemoryStream();
        gdiImg.Save(memory, ImageFormat.Png);
        var bitmapImage = new BitmapImage();
        bitmapImage.BeginInit();
        memory.Seek(0, SeekOrigin.Begin);
        bitmapImage.StreamSource = memory;
        bitmapImage.EndInit();

        return bitmapImage;
    }
}
