using MemoryPack;
using System.Diagnostics.CodeAnalysis;

namespace CoreLibrary.Services;

[MemoryPackable]
[SuppressMessage("ReSharper", "PartialTypeWithSinglePart", Justification = "Required by Mempack serializer")]
public partial record GeneratedImagesList(List<GeneratedImage> Images);

[MemoryPackable]
public partial class GeneratedImage(string base64EncodedImage, string promptText, int cfgScale)
{
    public string Base64EncodedImage { get; } = base64EncodedImage;
    public string PromptText { get; } = promptText;
    public int CfgScale { get; } = cfgScale;
}
