using CoreLibrary.Utilities;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;

namespace CoreLibrary.Services;

public record ImageGeneratorSettings(string CacheFolder);

/// <summary>
/// Calls API of AUTOMATIC1111's stable-diffusion-webui to generate good-looking images.
/// </summary>
public class ImageGenerator(HttpClient httpClient, ILogger<ImageGenerator> logger, ImageGeneratorSettings settings)
{
    /// <param name="cfgScale">Reasonable range seems from 2.0 (creative freedom) to 7.0 (already strictly following the prompt)</param>
    public async Task<List<GeneratedImage>> GenerateImageBatch(
        StableDiffusionPrompt stableDiffusionPrompt, int numImagesToGenerate, int cfgScale)
    {
        // Call API of AUTOMATIC1111's stable-diffusion-webui
        bool cutCornersForFasterResponseInDevelopment = false;

        var width = 1024;
        var height = 1024;

        var samplerName = "DPM++ 2M";
        var seed = 30456;
        var modelCheckpointId = new OverrideSettingsModel("sd_xl_base_1.0");

        // Cut corners in development to get faster response
        var numSteps = cutCornersForFasterResponseInDevelopment ? 10 : 24;
        var refinerCheckpointId = cutCornersForFasterResponseInDevelopment ? null : "sd_xl_refiner_1.0";
        decimal? refinerSwitchAt = cutCornersForFasterResponseInDevelopment ? null : 0.7m;

        var requestPayloadModel = new TextToImageRequestModel(
            stableDiffusionPrompt.PromptText,
            stableDiffusionPrompt.NegativePromptText,
            width, height, numImagesToGenerate, numSteps, cfgScale, samplerName,
            seed, modelCheckpointId, refinerCheckpointId, refinerSwitchAt);
        var cacheFileName = GenerateCacheFileName(requestPayloadModel);

        if (File.Exists(cacheFileName))
        {
            logger.LogInformation("Using cached images for prompt {Prompt}", stableDiffusionPrompt.PromptText);
            var cachedResultSerialized = await File.ReadAllTextAsync(cacheFileName);
            var cachedResult = JsonSerializer.Deserialize<List<GeneratedImage>>(cachedResultSerialized);
            if (cachedResult == null)
                throw new InvalidOperationException($"Failed to deserialize images cached in {cacheFileName}");

            return cachedResult;
        }

        // Cache miss -> call Stable Diffusion API
        await EnsureProperSetupOfHttpClient();
        await EnsureStableDiffusionApiIsRunning();

        var response = await httpClient.PostAsJsonAsync("http://localhost:7860/sdapi/v1/txt2img", requestPayloadModel);
        var responseModel = await response.Content.ReadFromJsonAsync<TextToImageResponseModel>();
        if (responseModel == null)
        {
            logger.LogError("Failed to generate image. Response was empty.");
            return new List<GeneratedImage>();
        }

        var arrayOfGenImages = responseModel.Images.Select(i => new GeneratedImage(i, stableDiffusionPrompt.PromptText, cfgScale)).ToList();

        // cache the response
        await File.WriteAllTextAsync(cacheFileName, JsonSerializer.Serialize(arrayOfGenImages));
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
            }

            await Task.Delay(TimeSpan.FromSeconds(retryTimeSeconds));
        }

        _ensuredStableDiffusionApiIsAlive = true;
    }


    private string GenerateCacheFileName(TextToImageRequestModel request)
    {
        settings.CacheFolder.EnsureDirectoryExists();

        var serializedRequest = JsonSerializer.Serialize(request);
        var fingerprint = serializedRequest.GetHashCodeStable(5);
        return Path.Combine(settings.CacheFolder,
            $"{request.Prompt.ToFilenameFriendlyString(20)}_{request.Width}x{request.Height}_{request.NumImages}_{fingerprint}.json");
    }


}
