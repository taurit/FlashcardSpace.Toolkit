using Microsoft.Extensions.Logging;

namespace CoreLibrary.Services;

public record StableDiffusionPrompt(string PromptText, string NegativePromptText);

public class StableDiffusionPromptProvider(ILogger logger)
{
    private const string NegativePromptText = "lowres,bad anatomy,bad hands,text," +
                                              "error,missing fingers,extra digit,fewer digits,cropped," +
                                              "worst quality,low quality,normal quality,jpeg artifacts," +
                                              "signature,watermark,username,blurry,nsfw,";
    string[] _keywords = {
        "abstract", "vibrant", "minimalist", "monochrome", "surreal",
        "geometric", "textured", "figurative", "dreamlike", "whimsical",
        "bold", "organic", "dynamic", "symmetrical", "ethereal",
        "harmonious", "rustic", "contrasting", "evocative", "grunge",
        "melancholic", "mystical", "serene", "playful", "futuristic",
        "intricate", "subtle", "chaotic", "timeless", "fragmented"
    };

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
        var random = new Random();
        for (int i = 0; i < 5; i++)
        {
            var randomIndex = random.Next(_keywords.Length);
            keywords.Add(_keywords[randomIndex]);
        }

        var promptText = String.Join(",", keywords);
        var prompt = new StableDiffusionPrompt(promptText, NegativePromptText);
        return prompt;
    }
}
