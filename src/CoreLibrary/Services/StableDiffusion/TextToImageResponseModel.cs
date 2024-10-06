using Newtonsoft.Json;

namespace CoreLibrary.Services.StableDiffusion;
record TextToImageResponseModel(
    [JsonProperty("images")] string[] Images,
    [JsonProperty("info")] string InfoFromModel
    );
