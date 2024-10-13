using CoreLibrary.Utilities;
using Microsoft.Extensions.Logging;

namespace CoreLibrary.Services.GenerativeAiClients.StableDiffusion;

public class ImageCandidatesGenerator(
        ImageGenerator imageGenerator,
        StableDiffusionPromptProvider promptProvider,
        ILogger<ImageCandidatesGenerator> logger)
{
    public async Task<List<GeneratedImage>> GenerateImageVariants(string termEnglish, string sentenceEnglish)
    {
        //var basePrompt = promptProvider.CreateGoodPrompt(termEnglish, sentenceEnglish, null, false);

        const int cfgScaleMin = 3;
        const int cfgScaleMax = 6;
        const int numExperiments = 4;
        const int numImagesInExperiment = 2;
        SupportedSDXLImageSize imageSize = SupportedSDXLImageSize.Wide;

        const int totalNumImages = numExperiments * numImagesInExperiment;

        //var results = await FindExistingImagesThatFit(basePrompt, imageSize, totalNumImages);
        var results = new List<GeneratedImage>();

        if (results.Count >= totalNumImages)
        {
            logger.LogInformation("Found enough existing images in the repo that match the prompt. Skipping image generation to save time.");
            return results;
        }

        decimal cfgScaleStep = ((decimal)cfgScaleMax - cfgScaleMin) / (numExperiments - 1);

        for (int i = 0; i < numExperiments; i++)
        {
            var seed = i + sentenceEnglish.GetHashCodeStableInt(); // add deterministic variety to the seed
            // ... to avoid generating 1000 images with the 'horror,gothic' theme etc.

            var prompt = promptProvider.CreateGoodPrompt(termEnglish, sentenceEnglish, seed);
            int cfgScaleForExperiment = cfgScaleMin + (int)(cfgScaleStep * i);

            logger.LogInformation("CFG={CfgScale}, Prompt: {Prompt}", cfgScaleForExperiment, prompt.PromptText);


            var images = await imageGenerator.GenerateImageBatch(prompt, numImagesInExperiment, cfgScaleForExperiment, seed, imageSize, ImageQualityProfile.DraftProfile);

            results.AddRange(images);
        }

        return results;
    }
}
