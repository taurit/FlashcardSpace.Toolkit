namespace CoreLibrary.Services;

public class GeneratedImage(string base64EncodedImage, string promptText)
{
    public string Base64EncodedImage { get; } = base64EncodedImage;
    public string PromptText { get; } = promptText;
}
