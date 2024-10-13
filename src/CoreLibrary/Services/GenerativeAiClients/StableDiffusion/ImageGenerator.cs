using CoreLibrary.Utilities;
using MemoryPack;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CoreLibrary.Services.GenerativeAiClients.StableDiffusion;

public record ImageGeneratorSettings(string CacheFolder);

/// <summary>
/// Calls API of AUTOMATIC1111's stable-diffusion-webui to generate good-looking images.
/// </summary>
public class ImageGenerator(HttpClient httpClient, ILogger<ImageGenerator> logger, ImageGeneratorSettings settings)
{
    static readonly JsonSerializerOptions StableDiffusionRequestSerializerOptions = new()
    {
        // serialize enum to string, otherwise the API will ignore enum parameters or throw an error
        Converters = { new JsonStringEnumConverter(allowIntegerValues: false) },
        IgnoreReadOnlyProperties = false
    };

    static readonly JsonSerializerOptions StableDiffusionRequestSerializerOptionsForCacheFingerprint = new()
    {
        // This is just because I don't want to invalidate my database of generated images; can be removed in the future and left with defaults
        // if I want to break the compatibility
        IgnoreReadOnlyProperties = true
    };

    /// <param name="cfgScale">Reasonable range seems from 2.0 (creative freedom) to 7.0 (already strictly following the prompt)</param>
    public async Task<List<GeneratedImage>> GenerateImageBatch(
            StableDiffusionPrompt stableDiffusionPrompt, int numImagesToGenerate, decimal cfgScale, Int64 seed,
            SupportedSDXLImageSize size, ImageQualityProfile qualityProfile)
    {
        var samplerName = "DPM++ 2M";
        var tensorRtOption = qualityProfile.IsRefinerEnabled ? TensorRtSetting.None : TensorRtSetting.Automatic;
        var modelCheckpointId = new OverrideSettingsModel("sd_xl_base_1.0", tensorRtOption);
        var refinerCheckpointId = qualityProfile.IsRefinerEnabled ? "sd_xl_refiner_1.0" : null;
        decimal? refinerSwitchAt = 0.7m;

        var requestPayloadModel = new TextToImageRequestModel(
            stableDiffusionPrompt.PromptText,
            stableDiffusionPrompt.NegativePromptText,
            size.Width, size.Height,
            numImagesToGenerate,
            qualityProfile.Steps,
            cfgScale,
            samplerName,
            seed,
            modelCheckpointId,
            refinerCheckpointId,
            refinerSwitchAt,
            qualityProfile.IsFaceRestorationEnabled);

        var cacheFileName = GenerateCacheFileName(requestPayloadModel);
        if (File.Exists(cacheFileName))
        {
            logger.LogDebug("Loading cached images for prompt {Prompt}", stableDiffusionPrompt.PromptText);
            var mempackCacheContent = await File.ReadAllBytesAsync(cacheFileName);
            var mempackContent = MemoryPackSerializer.Deserialize<GeneratedImagesList>(mempackCacheContent);
            return mempackContent!.Images;
        }

        // Cache miss -> call Stable Diffusion API
        await EnsureProperSetupOfHttpClient();
        await EnsureStableDiffusionApiIsRunning();

        Stopwatch sw = Stopwatch.StartNew();

        var response = await httpClient.PostAsJsonAsync("http://localhost:7860/sdapi/v1/txt2img", requestPayloadModel, StableDiffusionRequestSerializerOptions);
        var responseModel = await response.Content.ReadFromJsonAsync<TextToImageResponseModel>();
        sw.Stop();

        if (responseModel == null)
        {
            logger.LogError("Failed to generate image. Response was empty.");
            return new List<GeneratedImage>();
        }
        else
        {
            logger.LogInformation("Generated {NumImages} images in {TimeMs} ms", responseModel.Images.Length, sw.ElapsedMilliseconds);
        }

        var arrayOfGenImages = responseModel.Images.Select(i => new GeneratedImage(i, stableDiffusionPrompt.PromptText, cfgScale)).ToList();

        // cache the response
        var imageList = new GeneratedImagesList(arrayOfGenImages);
        var newCacheContent = MemoryPackSerializer.Serialize(imageList);
        await File.WriteAllBytesAsync(cacheFileName, newCacheContent);

        return arrayOfGenImages;
    }


    /// <summary>
    /// This method:
    /// 1) Reads the Stable Diffusion prompt and parameters from the JPEG file metadata
    /// 2) If the parameters fall into the "quick draft" category, it generates a new image with the same prompt and seed
    ///    (for identical composition) but with more steps and time-consuming settings like refiner and face restoration enabled.
    /// </summary>
    public async Task<bool> ImproveImageQualityIfNeeded(string filePath)
    {
        var fileParams = StableDiffusionHelper.GetStableDiffusionParametersFromImage(filePath);
        if (fileParams is null)
        {
            logger.LogWarning($"Failed to read Stable Diffusion parameters from image {filePath}");
            throw new InvalidOperationException($"Failed to read Stable Diffusion parameters from image {filePath}");
        }

        var restoreFaces = !String.IsNullOrWhiteSpace(fileParams.FaceRestoration);
        var isBelowHighQualityBar = ImageQualityProfile.IsBelowHighQualityBar(fileParams.Steps, fileParams.Refiner, restoreFaces);

        if (isBelowHighQualityBar)
        {
            logger.LogInformation("Improving image quality for {FilePath}", filePath);

            var newPrompt = new StableDiffusionPrompt(fileParams.Prompt, fileParams.NegativePrompt);
            var newSize = SupportedSDXLImageSize.FromWidthAndHeight(fileParams.Width, fileParams.Height);

            var newImage = await GenerateImageBatch(newPrompt, 1, fileParams.CfgScale, fileParams.Seed, newSize, ImageQualityProfile.HighQualityProfile);

            if (newImage is null)
                return false;

            var newImageBase64 = newImage.First().Base64EncodedImage;
            var newImageBinary = Convert.FromBase64String(newImageBase64);
            var newFilePath = filePath.Replace(".jpg", "_improved.jpg");

            await File.WriteAllBytesAsync(newFilePath, newImageBinary);

            return true;
        }
        return false;
    }

    private bool _timeoutAlreadySet = false;
    private async Task EnsureProperSetupOfHttpClient()
    {
        if (_timeoutAlreadySet)
            return;
        httpClient.Timeout = TimeSpan.FromMinutes(5);
        _timeoutAlreadySet = true;
    }

    private bool _ensuredStableDiffusionApiIsAlive = false;
    private async Task EnsureStableDiffusionApiIsRunning()
    {
        if (_ensuredStableDiffusionApiIsAlive)
            return;

        var isAlive = false;
        const int retryTimeSeconds = 5;

        while (!isAlive)
        {
            isAlive = await StableDiffusionHelper.IsAlive();
            if (!isAlive)
            {
                logger.LogWarning("Stable Diffusion API is not running. Retrying in {RetryTimeSeconds} seconds...", retryTimeSeconds);
                await Task.Delay(TimeSpan.FromSeconds(retryTimeSeconds));
            }
        }

        var configurationErrors = await StableDiffusionHelper.ValidateStableDiffusionApiGlobalOptions();
        if (!String.IsNullOrEmpty(configurationErrors))
        {
            logger.LogError("Stable Diffusion API is not correctly configured. Errors: {Errors}", configurationErrors);
            throw new InvalidOperationException($"Stable Diffusion API is not correctly configured: {configurationErrors}.");
        }

        logger.LogInformation("Stable Diffusion API is correctly configured.");
        _ensuredStableDiffusionApiIsAlive = true;
    }


    private string GenerateCacheFileName(TextToImageRequestModel request)
    {
        settings.CacheFolder.EnsureDirectoryExists();

        var serializedRequest = JsonSerializer.Serialize(request, StableDiffusionRequestSerializerOptionsForCacheFingerprint);
        var fingerprint = serializedRequest.GetHashCodeStable(5);
        return Path.Combine(settings.CacheFolder,
            $"{request.Prompt.ToFilenameFriendlyString(20)}_{request.Width}x{request.Height}_{request.NumImages}_{fingerprint}.mempack");
    }
}
