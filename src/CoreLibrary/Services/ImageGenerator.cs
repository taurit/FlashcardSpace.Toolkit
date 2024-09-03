using System.Net.Http.Json;

namespace CoreLibrary.Services;

/// <summary>
/// Calls API of AUTOMATIC1111's stable-diffusion-webui to generate good looking images.
/// </summary>
public class ImageGenerator(HttpClient httpClient)
{
    public async Task<GeneratedImage> GenerateImage(string sentenceToIllustrateInEnglish)
    {
        return await GenerateImage();
    }

    private async Task<GeneratedImage> GenerateImage()
    {
        // Call API of AUTOMATIC1111's stable-diffusion-webui

        var requestPayloadModel = new TextToImageRequestModel(
            "masterpiece,best quality,<lora:tbh323-sdxl:0.6>,cat in fisheye,flowers,<lora:tbh123-sdxl:0.2>,paint by Vincent van Gogh,",
            "lowres,bad anatomy,bad hands,text,error,missing fingers,extra digit,fewer digits,cropped,worst quality,low quality,normal quality,jpeg artifacts,signature,watermark,username,blurry,nsfw,",
            1024, 1024, 1, 24, 5, "DPM++ 2M", 30456, new OverrideSettingsModel("sd_xl_base_1.0"), "sd_xl_refiner_1.0", 0.7m);

        // Call API
        var response = await httpClient.PostAsJsonAsync("http://localhost:7860/sdapi/v1/txt2img", requestPayloadModel);
        var responseModel = await response.Content.ReadFromJsonAsync<TextToImageResponseModel>();

        return new GeneratedImage(responseModel.Images[0]);

    }

}
