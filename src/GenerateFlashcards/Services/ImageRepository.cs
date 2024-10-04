using CoreLibrary.Models;
using CoreLibrary.Services;

namespace GenerateFlashcards.Services;


/// <summary>
/// Allows search for image candidates matching the prompt in the repository of previously generated images.
/// Potentially allows to save time on image generation which is by far the most time-consuming part of flashcard generation.
/// </summary>
internal class ImageRepository(ImageRepositorySettings settings)
{
    private readonly List<StableDiffusionImage>? _images = null;
    private List<StableDiffusionImage> LoadExistingImagesMetadata()
    {
        if (_images is not null)
            return _images;

        var images = new List<StableDiffusionImage>();

        var files = Directory.GetFiles(settings.ImageRepositoryFolder, "*.jpg");
        foreach (var file in files)
        {
            var parameters = StableDiffusionHelper.GetStableDiffusionParametersFromImage(file);
            if (parameters is null)
            {
                continue;
            }
            images.Add(new StableDiffusionImage(file, parameters));
        }
        return images;
    }

    internal List<StableDiffusionImage> FindMatchingImageCandidates(string prompt, int width, int height, ImageGenerationProfile minimumQuality)
    {
        // filter by width and height first
        var images = LoadExistingImagesMetadata();
        images = images.Where(x => x.Parameters.Width == width && x.Parameters.Height == height).ToList();

        // filter by quality
        if (minimumQuality == ImageGenerationProfile.PublicDeck)
        {
            images = images.Where(x => x.Parameters.Steps > 20).ToList();
        }

        // filter by prompt - use Embeddings to match the prompt with some threshold

        return images;
    }
}

internal record ImageRepositorySettings(string ImageRepositoryFolder);
internal record StableDiffusionImage(string FilePath, StableDiffusionParameters Parameters);
