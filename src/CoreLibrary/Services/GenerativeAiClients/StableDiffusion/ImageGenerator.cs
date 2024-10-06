using CoreLibrary.Utilities;
using MemoryPack;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;

namespace CoreLibrary.Services.GenerativeAiClients.StableDiffusion;

public record ImageGeneratorSettings(string CacheFolder);

/// <summary>
/// Calls API of AUTOMATIC1111's stable-diffusion-webui to generate good-looking images.
/// </summary>
public class ImageGenerator(HttpClient httpClient, ILogger<ImageGenerator> logger, ImageGeneratorSettings settings)
{
    /// <param name="cfgScale">Reasonable range seems from 2.0 (creative freedom) to 7.0 (already strictly following the prompt)</param>
    public async Task<List<GeneratedImage>> GenerateImageBatch(
            StableDiffusionPrompt stableDiffusionPrompt, int numImagesToGenerate, int cfgScale, int seed,
            SupportedSDXLImageSize size)
    {
        var samplerName = "DPM++ 2M";
        var modelCheckpointId = new OverrideSettingsModel("sd_xl_base_1.0");

        // Cut corners in development to get faster response
        var numSteps = 24;
        var refinerCheckpointId = "sd_xl_refiner_1.0";
        decimal? refinerSwitchAt = 0.7m;

        var requestPayloadModel = new TextToImageRequestModel(
            stableDiffusionPrompt.PromptText,
            stableDiffusionPrompt.NegativePromptText,
            size.Width, size.Height, numImagesToGenerate, numSteps, cfgScale, samplerName,
            seed, modelCheckpointId, refinerCheckpointId, refinerSwitchAt, RestoreFaces: true);

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
        var response = await httpClient.PostAsJsonAsync("http://localhost:7860/sdapi/v1/txt2img", requestPayloadModel);
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

        var serializedRequest = JsonSerializer.Serialize(request);
        var fingerprint = serializedRequest.GetHashCodeStable(5);
        return Path.Combine(settings.CacheFolder,
            $"{request.Prompt.ToFilenameFriendlyString(20)}_{request.Width}x{request.Height}_{request.NumImages}_{fingerprint}.mempack");
    }


}
