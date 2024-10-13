using MemoryPack;
using System.Diagnostics.CodeAnalysis;

namespace CoreLibrary.Services.GenerativeAiClients.StableDiffusion;

[MemoryPackable]
[SuppressMessage("ReSharper", "PartialTypeWithSinglePart", Justification = "Required by Mempack serializer")]
public partial record GeneratedImagesList(List<GeneratedImage> Images);

[MemoryPackable]
public partial class GeneratedImage(string base64EncodedImage, string promptText, decimal cfgScale)
{
    public string Base64EncodedImage { get; } = base64EncodedImage;
    public string PromptText { get; } = promptText;
    public decimal CfgScale { get; } = cfgScale;
}
