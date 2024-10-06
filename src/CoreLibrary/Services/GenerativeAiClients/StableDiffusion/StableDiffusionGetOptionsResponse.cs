using System.Text.Json.Serialization;

namespace CoreLibrary.Services.GenerativeAiClients.StableDiffusion;

/// <summary>
/// A partial model of a response from /sdapi/v1/options.
/// </summary>
class StableDiffusionGetOptionsResponse
{
    [JsonPropertyName("samples_format")]
    public string SamplesFormat { get; set; }
}
