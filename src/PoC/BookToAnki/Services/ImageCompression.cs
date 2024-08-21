using System.Diagnostics;

namespace BookToAnki.Services;

public class ImageCompression
{

    public void ConvertToWebpAndRemoveOriginalImage(string inputFilePath)
    {
        ConvertToWebp(inputFilePath, 70);
        ConvertToWebp(inputFilePath, 100);
    }

    // Webp is selected for wide-enough compatibility with Anki (Windows, Android, iOS since 2020 too but I had no chance to test yet).
    // so this is a format for flashcards
    // I tested that AVIF is not supported anywhere in Anki, though.
    private static void ConvertToWebp(String inputFile, int quality)
    {
        var outputFile = Path.ChangeExtension(inputFile, $".{quality}.webp");
        var process = new Process();
        process.StartInfo.FileName = "magick";
        process.StartInfo.Arguments =
            $"convert \"{inputFile}\" -auto-orient -quality {quality} -define webp:lossless=false -define webp:method=6 \"{outputFile}\"";

        process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
        process.Start();
        process.WaitForExit();
    }

}

