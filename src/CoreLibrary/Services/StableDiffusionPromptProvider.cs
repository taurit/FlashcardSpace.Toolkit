using Microsoft.Extensions.Logging;

namespace CoreLibrary.Services;

public record StableDiffusionPrompt(string PromptText, string NegativePromptText);

public class StableDiffusionPromptProvider(ILogger logger)
{
    private const string NegativePromptText = "lowres,bad anatomy,bad hands,text," +
                                              "error,missing fingers,extra digit,fewer digits,cropped," +
                                              "worst quality,low quality,normal quality,jpeg artifacts," +
                                              "signature,watermark,username,blurry,nsfw,";

    readonly string[] _keywords = {
        "vintage", "modern", "industrial", "pastel", "nocturnal",
        "reflective", "symbolic", "angular", "melodic", "flowing",
        "layered", "muted", "harmonic", "radiant", "mechanical",
        "distorted", "glossy", "ornate", "delicate", "rigid",
        "mechanistic", "tranquil", "organic", "faded", "opulent",
        "glowing", "sharp", "blurry", "colorful", "bleak",
        "detailed", "simple", "chaotic", "intriguing", "futuristic",
        "rusted", "serpentine", "cubist", "bizarre", "muted",
        "saturated", "airy", "contemplative", "medieval", "prismatic",
        "deconstructed", "fantastical", "static", "angular", "lyrical",
        "earthy", "mechanical", "repetitive", "interlocking", "woven",
        "sketchy", "glitchy", "psychedelic", "translucent", "iridescent",
        "vivid", "cinematic", "pop", "tactile", "monolithic",
        "gothic", "baroque", "renaissance", "expressive", "raw",
        "naturalistic", "modular", "digital", "fluid", "turbulent",
        "static", "sculptural", "volumetric", "mesmerizing", "sharp-edged",
        "smeared", "bleached", "dreamy", "fragmentary", "grotesque",
        "cubist", "polished", "etched", "ornamental", "bleak",
        "sharp-edged", "moody", "transcendent", "warped", "eroded",
        "primitive", "alien", "urban", "timid", "glacial",
        "brutal", "ethnic", "impressionistic", "avant-garde", "playful" };

    /// <remarks>
    /// Remarks: example of a prompt giving good visual results:
    ///
    /// `masterpiece,best quality,<lora:tbh323-sdxl:0.6>,cat in fisheye,flowers,
    /// <lora:tbh123-sdxl:0.2>,paint by Vincent van Gogh`
    /// </remarks>
    public StableDiffusionPrompt CreateGoodPrompt(string termEnglish, string sentenceEnglish, int seed)
    {
        var random = new Random(seed);
        var keywords = new List<string>();

        // always add the main sentence
        keywords.Add(sentenceEnglish);

        // and the main keyword might help too
        keywords.Add(termEnglish);

        // then use few random keywords to make the prompt more diverse and experiment with the style
        for (int i = 0; i < 4; i++)
        {
            var randomIndex = random.Next(_keywords.Length);
            keywords.Add(_keywords[randomIndex]);
        }

        var promptText = String.Join(",", keywords);
        var prompt = new StableDiffusionPrompt(promptText, NegativePromptText);
        return prompt;
    }
}
