using BookToAnki.Models;
using OpenAI;
using OpenAI.Managers;
using OpenAI.ObjectModels;
using OpenAI.ObjectModels.RequestModels;

namespace BookToAnki.Services.OpenAi;

public class DalleServiceWrapper
{
    private readonly OpenAIService _openAiService;

    public DalleServiceWrapper(string developerKey, string organization)
    {
        _openAiService = new OpenAIService(new OpenAiOptions()
        {
            ApiKey = developerKey,
            Organization = organization
        });
    }

    public async Task<List<DalleImage>> CreateDalle2Image(string prompt, string size, int numImages = 1)
    {
        List<DalleImage> resultImages = new List<DalleImage>(numImages);
        var imageResult = await _openAiService.Image.CreateImage(new ImageCreateRequest
        {
            Prompt = prompt,
            N = numImages,
            Size = size,
            ResponseFormat = StaticValues.ImageStatics.ResponseFormat.Base64,
            User = "Taurit",
        });

        if (imageResult.Successful)
        {
            foreach (var image in imageResult.Results)
            {
                var dalleImage = new DalleImage(image.B64);
                resultImages.Add(dalleImage);
            }
            return resultImages;
        }

        if (imageResult.Error is not null)
        {
            throw new InvalidOperationException($"Generating Dall-E image failed: {imageResult.Error.Code}: {imageResult.Error.Message}");
        }

        throw new InvalidOperationException("Generating Dall-E image failed, but the service didn't provide any details");
    }

    public enum Dalle3ImageQuality { hd, standard }
    public async Task<DalleImage> CreateDalle3Image(string prompt, Dalle3ImageQuality quality)
    {
        var imageResult = await _openAiService.Image.CreateImage(new ImageCreateRequest
        {
            Prompt = prompt,
            N = 1,
            Size = StaticValues.ImageStatics.Size.Size1024,
            ResponseFormat = StaticValues.ImageStatics.ResponseFormat.Base64,
            User = "Taurit",
            Model = OpenAI.ObjectModels.Models.Dall_e_3,
            Quality = quality.ToString(),
            Style = "vivid" // "natural" or "vivid" which is the default
        });

        if (imageResult.Successful)
        {
            var dalleImage = new DalleImage(imageResult.Results.Single().B64);
            return dalleImage;
        }

        if (imageResult.Error is not null)
        {
            throw new InvalidOperationException($"Generating Dall-E image failed: {imageResult.Error.Code}: {imageResult.Error.Message}");
        }

        throw new InvalidOperationException("Generating Dall-E image failed, but the service didn't provide any details");
    }

}
