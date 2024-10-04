using CoreLibrary.Models;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;

namespace CoreLibrary.Services;

// A small util (stateless service) to interact with the local Stable Diffusion API's functions  
// other than txt2img image generation (which has a dedicated service).  
public static class StableDiffusionHelper
{
    /// <summary>  
    /// Tests if local Stable Diffusion API is running.  
    ///  
    /// For image generation, this service must be configured and running locally:  
    /// https://github.com/AUTOMATIC1111/stable-diffusion-webui/  
    /// </summary>  
    public static async Task<bool> IsAlive()
    {
        var testUrl = "http://127.0.0.1:7860/favicon.ico";
        var timeout = TimeSpan.FromSeconds(1);
        var testHttpClient = new HttpClient { Timeout = timeout };
        try
        {
            var response = await testHttpClient.GetAsync(testUrl);
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {
            return false;
        }
    }


    public static StableDiffusionParameters? GetStableDiffusionParametersFromImage(string filePath)
    {
        // Read all metadata from the image
        var directories = ImageMetadataReader.ReadMetadata(filePath);

        // Find the ExifSubIFDDirectory which contains the UserComment tag
        var exifDirectory = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();

        if (exifDirectory != null)
        {
            var userComment = exifDirectory.Tags.FirstOrDefault(x => x.Name == "User Comment")?.Description;
            var result = (userComment is not null) ? StableDiffusionParameters.FromString(userComment) : null;
            return result;
        }

        return null;
    }
}
