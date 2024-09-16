using Microsoft.Extensions.Logging;

namespace CoreLibrary.Services;

public record StableDiffusionPrompt(string PromptText, string NegativePromptText);

public class StableDiffusionPromptProvider(ILogger logger)
{
    private const string NegativePromptText = "lowres,bad anatomy,bad hands,text," +
                                              "error,missing fingers,extra digit,fewer digits,cropped," +
                                              "worst quality,low quality,normal quality,jpeg artifacts," +
                                              "signature,watermark,username,blurry,nsfw,";

    /// <remarks>
    /// Remarks: example of a prompt giving good visual results:
    ///
    /// `masterpiece,best quality,<lora:tbh323-sdxl:0.6>,cat in fisheye,flowers,
    /// <lora:tbh123-sdxl:0.2>,paint by Vincent van Gogh`
    /// </remarks>
    public StableDiffusionPrompt CreateGoodPrompt(string termEnglish, string sentenceEnglish)
    {
        var keywords = new List<string>();

        keywords.Add(sentenceEnglish);

        var promptText = String.Join(",", keywords);
        var prompt = new StableDiffusionPrompt(promptText, NegativePromptText);
        return prompt;
    }
}
