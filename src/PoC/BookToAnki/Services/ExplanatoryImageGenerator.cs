using BookToAnki.Services.OpenAi;
using OpenAI.ObjectModels;
using System.Text;

namespace BookToAnki.Services;

public class ExplanatoryImageGenerator
{
    private readonly DalleServiceWrapper _dalleService;
    private readonly string _imagesRootFolder;
    private readonly OpenAiServiceWrapper _openAiService;

    public ExplanatoryImageGenerator(OpenAiServiceWrapper openAiService, DalleServiceWrapper dalleService,
        string imagesRootFolder)
    {
        _openAiService = openAiService;
        _dalleService = dalleService;
        _imagesRootFolder = imagesRootFolder;
    }

    [Obsolete("Uses DALL-e 2, and requires separate call to generate image prompts")]
    public async Task BatchGenerateImages(IReadOnlyList<WordToExplain> words)
    {
        var systemPrompt = await File.ReadAllTextAsync("Resources/GenerateDallePrompts.System.txt");

        foreach (var chunk in words.Chunk(10))
        {
            var userPrompt = new StringBuilder("Words to process:\n");

            foreach (var word in chunk) userPrompt.AppendLine($"- `{word.Word}` as in the context `{word.Context}`.");

            var userPromptString = userPrompt.ToString();
            var responseWithPrompts = await _openAiService.CreateChatCompletion(systemPrompt, userPromptString, OpenAI.ObjectModels.Models.Gpt_4o, false);
            if (responseWithPrompts is null) throw new ArgumentException("ChatGPT provided no valid response");

            var dallePrompts = responseWithPrompts.Split(Environment.NewLine,
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (dallePrompts.Length != chunk.Length)
                throw new ArgumentException(
                    $"Unexpected response - expected {chunk.Length} answers, but got {dallePrompts.Length}");

            for (var promptIndex = 0; promptIndex < dallePrompts.Length; promptIndex++)
            {
                var prompt = dallePrompts[promptIndex].Trim('\"', ' ', '.', '-');
                var word = chunk[promptIndex];

                var wordImageFolderPath = Path.Combine(_imagesRootFolder, word.Word.ToLowerInvariant());
                if (Directory.Exists(wordImageFolderPath)) continue;

                Directory.CreateDirectory(wordImageFolderPath);
                var images = await _dalleService.CreateDalle2Image(prompt, StaticValues.ImageStatics.Size.Size1024, 1);

                for (var imageIndex = 0; imageIndex < images.Count; imageIndex++)
                {
                    var image = images[imageIndex];
                    var imageFileName = Path.Combine(wordImageFolderPath, $"dalle-{imageIndex}.png");
                    var imageBytes = Convert.FromBase64String(image.ImageContentBase64);
                    await File.WriteAllBytesAsync(imageFileName, imageBytes);
                }
            }
        }
    }
}
