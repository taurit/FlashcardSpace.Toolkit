using Microsoft.Extensions.Logging;

namespace CoreLibrary.Services;

public class ImageCandidatesGenerator(
        ImageGenerator imageGenerator,
        StableDiffusionPromptProvider promptProvider,
        ILogger<ImageCandidatesGenerator> logger)
{
    public async Task<List<GeneratedImage>> GenerateImageVariants(string termEnglish, string sentenceEnglish,
        int numExperiments, int numImagesInExperiment)
    {
        var results = new List<GeneratedImage>();

        const int cfgScaleMin = 3;
        const int cfgScaleMax = 7;

        decimal cfgScaleStep = numExperiments == 1 ? 0 : ((decimal)cfgScaleMax - cfgScaleMin) / (numExperiments - 1);

        for (int i = 0; i < numExperiments; i++)
        {
            logger.LogInformation("Generating images for experiment {ExperimentNumber}/{NumExperiments}", i + 1, numExperiments);

            var prompt = promptProvider.CreateGoodPrompt(termEnglish, sentenceEnglish, i);
            int cfgScaleForExperiment = cfgScaleMin + (int)(cfgScaleStep * i);

            logger.LogDebug("CFG={CfgScale}, Prompt: {Prompt}", cfgScaleForExperiment, prompt.PromptText);
            var images = await imageGenerator.GenerateImageBatch(prompt, numImagesInExperiment, cfgScaleForExperiment);

            results.AddRange(images);
        }

        return results;
    }
}
