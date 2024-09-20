using ImageMagick;

namespace Anki.NET.Helpers;
public class ImageProcessor
{
    internal void ConvertToWebpAndResize(string inputPath, string outputPath)
    {
        using var image = new MagickImage(inputPath);

        // Check if width is greater than or equal to 1080
        if (image.Width > 1080)
        {
            // Calculate new height to maintain aspect ratio
            var newHeight = (int)((1080 / (double)image.Width) * image.Height);

            // Resize the image to 1080px width while maintaining aspect ratio
            image.Resize(1080, (uint)newHeight);
        }

        // Convert the image to webp format
        image.Format = MagickFormat.WebP;
        image.Quality = 80;
        // Save the image
        image.Write(outputPath);
    }
}
