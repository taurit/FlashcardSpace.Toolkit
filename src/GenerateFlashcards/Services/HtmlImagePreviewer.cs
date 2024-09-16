using CoreLibrary.Services;
using System.Diagnostics;

namespace GenerateFlashcards.Services;

/// <summary>
/// A quick, temporary tool to generate HTML preview of image matrix to help developers
/// fine-tune the parameters for best quality.
/// </summary>
internal static class HtmlImagePreviewer
{
    public static async Task PreviewImages(IEnumerable<GeneratedImage> images)
    {
        var outputFilePath = Path.Combine(Path.GetTempPath(), "generate_flashcards_image_preview.html");
        var htmlFragment = "<html>\n" +
                           "<body>\n"
                           ;
        foreach (var image in images)
        {
            htmlFragment += $"<img src=\"data:image/png;base64,{image.Base64EncodedImage}\" /><br />\n";
        }
        htmlFragment += "</body>\n" +
                        "</html>\n";

        await File.WriteAllTextAsync(outputFilePath, htmlFragment);

        // Launch html
        var processStartInfo = new ProcessStartInfo(outputFilePath)
        {
            UseShellExecute = true
        };
        Process.Start(processStartInfo);

    }
}
