using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace CoreLibrary.Services.StableDiffusion;

/// <summary>
/// An element of the array in the response from `GET /sdapi/v1/sd-models`
/// </summary>
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global", Justification = "Used by deserializer")]
class StableDiffusionModelMetadata
{
    [JsonPropertyName("model_name")]
    public string ModelName { get; set; }
}
