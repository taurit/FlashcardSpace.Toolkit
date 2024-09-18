using CoreLibrary.Services;
using System.Diagnostics;
using System.Reflection;

namespace GenerateFlashcards.Services;

/// <summary>
/// A quick, temporary tool to generate HTML preview of image matrix to help developers
/// fine-tune the parameters for best quality.
/// </summary>
internal static class HtmlImagePreviewer
{
    public static async Task PreviewImages(IEnumerable<GeneratedImage> images)
    {
        var directoryPathOfRunningDll = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var stylesFileName = "HtmlImagePreviewer.css";
        var cssStylePath = Path.Combine(directoryPathOfRunningDll!, "Resources", stylesFileName);

        var previewerFolder = Path.Combine(Path.GetTempPath(), "HtmlImagePreviewer");
        Directory.CreateDirectory(previewerFolder);
        // copy styles to previewer directory unless they already exist there
        var cssStyleDestinationPath = Path.Combine(previewerFolder, stylesFileName);
        if (!File.Exists(cssStyleDestinationPath))
        {
            File.Copy(cssStylePath, cssStyleDestinationPath);
        }

        var outputFilePath = Path.Combine(previewerFolder, "preview.html");
        var htmlFragment = "<html>\n" +
                           "<head>\n" +
                           "    <link rel=\"stylesheet\" type=\"text/css\" href=\"" + stylesFileName + "\" />\n" +
                           "</head>\n" +
                           "<body>\n"
                           ;
        foreach (var image in images)
        {
            htmlFragment += $"<img src=\"data:image/jpeg;base64,{image.Base64EncodedImage}\" title=\"{image.PromptText}, cfg={image.CfgScale}\" />\n";
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

        // Launch css file in default editor
        processStartInfo = new ProcessStartInfo(cssStyleDestinationPath)
        {
            UseShellExecute = true
        };
        Process.Start(processStartInfo);

    }
}
