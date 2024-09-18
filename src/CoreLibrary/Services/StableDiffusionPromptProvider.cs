using Microsoft.Extensions.Logging;

namespace CoreLibrary.Services;

public record StableDiffusionPrompt(string PromptText, string NegativePromptText);

public class StableDiffusionPromptProvider(ILogger logger)
{
    private const string NegativePromptText = "lowres,bad anatomy,bad hands,text," +
                                              "error,missing fingers,extra digit,fewer digits,cropped," +
                                              "worst quality,low quality,jpeg artifacts," +
                                              "signature,watermark,username,blurry,nsfw,";

    readonly string[] _styles = {
        "vintage", "modern", "cubist", "industrial", "gothic", "baroque",
        "renaissance", "avant-garde", "impressionistic", "fantastical",
        "medieval", "digital", "futuristic", "pop", "expressionistic",
        "glamour", "studio-quality", "fashion", "cinematic", "vivid"
    };

    readonly string[] _moods = {
        "tranquil", "moody", "bleak", "dreamy", "playful", "grotesque",
        "contemplative", "melodic", "chaotic", "bleached", "eroded",
        "raw", "static", "turbulent", "surreal", "happy", "joyful",
        "energetic", "whimsical", "uplifting", "bright", "cheerful"
    };

    readonly string[] _textures = {
        "glossy", "matte", "ornate", "delicate", "rigid", "flowing",
        "distorted", "layered", "sharp", "faded", "colorful", "bleak",
        "angular", "airy", "woven", "fragmentary", "polished", "etched",
        "smooth", "velvety", "shiny", "radiant", "luminous", "crisp"
    };

    public StableDiffusionPrompt CreateGoodPrompt(string termEnglish, string sentenceEnglish, int? seed)
    {
        var random = new Random(seed.Value);
        var keywords = new List<string>();

        // always add the main sentence
        keywords.Add(sentenceEnglish);

        // add the main keyword
        keywords.Add(termEnglish);

        // add one from each category for diversity and coherence
        keywords.Add(_styles[random.Next(_styles.Length)]);
        keywords.Add(_moods[random.Next(_moods.Length)]);
        keywords.Add(_textures[random.Next(_textures.Length)]);

        // ensuring uniqueness
        var promptText = string.Join(",", keywords.Distinct());
        var prompt = new StableDiffusionPrompt(promptText, NegativePromptText);
        return prompt;
    }
}
