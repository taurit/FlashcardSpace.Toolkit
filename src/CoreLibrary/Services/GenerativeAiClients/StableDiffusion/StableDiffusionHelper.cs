using CoreLibrary.Models;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using System.Text.Json;

namespace CoreLibrary.Services.GenerativeAiClients.StableDiffusion;

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

    /// <summary>
    /// Tests if the local Stable Diffusion API is correctly configured, by which I mean specifically:
    /// - Output format of images is set to JPEG
    /// - The model named `sd_xl_base_1.0` is available
    /// - The model named `sd_xl_refiner_1.0` is available
    /// - Face restoration is enabled
    /// </summary>
    /// <returns></returns>
    public static async Task<string?> ValidateStableDiffusionApiGlobalOptions()
    {
        // validate options
        var getOptionsUrl = "http://127.0.0.1:7860/sdapi/v1/options";
        var timeout = TimeSpan.FromSeconds(5);
        var testHttpClient = new HttpClient { Timeout = timeout };
        var response = await testHttpClient.GetAsync(getOptionsUrl);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var options = JsonSerializer.Deserialize<StableDiffusionGetOptionsResponse>(content);

        if (options is null)
            return "Failed to deserialize the response from the Stable Diffusion API.";

        if (options.SamplesFormat != "jpg")
            return $"Output format is set to {options.SamplesFormat}, but `jpg` is expected by this tool.";

        if (options.FaceRestorationModel != "CodeFormer")
            return $"Face restoration model is set to {options.FaceRestorationModel}, but `CodeFormer` is expected by this tool.";

        // I don't remember changing any other defaults, but it might be extended if needed...

        // validate models availability
        var requiredModels = new[] { "sd_xl_base_1.0", "sd_xl_refiner_1.0" };
        var getModelsUrl = "http://127.0.0.1:7860/sdapi/v1/sd-models";
        response = await testHttpClient.GetAsync(getModelsUrl);
        response.EnsureSuccessStatusCode();
        content = await response.Content.ReadAsStringAsync();
        var models = JsonSerializer.Deserialize<StableDiffusionModelMetadata[]>(content);

        if (models is null)
            return "Failed to deserialize the response with models metadata from the Stable Diffusion API.";
        if (models.Length == 0)
            return "No models are available in the Stable Diffusion API.";

        var missingModels = requiredModels.Except(models.Select(x => x.ModelName)).ToArray();
        if (missingModels.Length > 0)
            return $"The following models are missing: {String.Join(", ", missingModels)}.";

        return null;
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


