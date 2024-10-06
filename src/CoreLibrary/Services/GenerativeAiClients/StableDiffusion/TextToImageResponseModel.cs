using Newtonsoft.Json;

namespace CoreLibrary.Services.GenerativeAiClients.StableDiffusion;
record TextToImageResponseModel(
    [JsonProperty("images")] string[] Images,
    [JsonProperty("info")] string InfoFromModel
    );
