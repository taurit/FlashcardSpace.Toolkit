using Microsoft.Extensions.Logging;

namespace CoreLibrary.Services.GenerativeAiClients.StableDiffusion;

public record StableDiffusionPrompt(string PromptText, string NegativePromptText);

public class StableDiffusionPromptProvider(ILogger logger)
{
    private const string NegativePromptText = "lowres,bad anatomy,bad hands,text," +
                                              "error,missing fingers,extra digit,fewer digits,cropped," +
                                              "worst quality,low quality,jpeg artifacts," +
                                              "signature,watermark,username,blurry,nsfw," +
                                              "painting,drawing,illustration,cartoon,anime,sketch,";

    readonly string[] _styles = {
        "high-resolution", "4K", "8K", "HDR", "photorealistic", "professional",
        "DSLR quality", "studio lighting", "golden hour", "blue hour", "natural light",
        "artificial lighting", "cinematic", "editorial", "fashion photography",
        "street photography", "landscape photography", "portrait photography",
        "product photography", "architectural photography", "macro photography",
        "bokeh", "shallow depth of field", "wide-angle", "telephoto", "drone shot"
    };

    readonly string[] _moods = {
        "tranquil", "moody", "atmospheric", "dramatic", "vibrant", "serene",
        "dynamic", "intimate", "bold", "minimalist", "rustic", "urban",
        "futuristic", "nostalgic", "romantic", "mysterious", "energetic",
        "calm", "ethereal", "powerful", "subtle", "raw", "polished"
    };

    readonly string[] _textures = {
        "smooth", "sharp", "crisp", "detailed", "glossy", "matte", "silky",
        "textured", "grainy", "soft focus", "high contrast", "low contrast",
        "vivid colors", "muted colors", "monochromatic", "saturated", "desaturated",
        "high clarity", "film grain", "long exposure", "motion blur", "tack sharp"
    };

    public StableDiffusionPrompt CreateGoodPrompt(string termEnglish, string sentenceEnglish, int? seed, bool addStyleKeywords = true)
    {
        if (addStyleKeywords && seed == null)
            throw new ArgumentException("Seed must be provided when adding style keywords");

        var keywords = new List<string>();

        // always add the main sentence
        keywords.Add(sentenceEnglish);

        // add the main keyword
        keywords.Add(termEnglish);

        if (addStyleKeywords)
        {
            var random = new Random(seed.Value);

            // add one from each category for diversity and coherence
            keywords.Add(_styles[random.Next(_styles.Length)]);
            keywords.Add(_moods[random.Next(_moods.Length)]);
            keywords.Add(_textures[random.Next(_textures.Length)]);
        }

        var promptText = string.Join(",", keywords);
        var prompt = new StableDiffusionPrompt(promptText, NegativePromptText);
        return prompt;
    }

}
