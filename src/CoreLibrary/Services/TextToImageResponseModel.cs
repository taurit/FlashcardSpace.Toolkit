using Newtonsoft.Json;

namespace CoreLibrary.Services;
record TextToImageResponseModel(
    [JsonProperty("images")] string[] Images,
    [JsonProperty("info")] string InfoFromModel
    );
