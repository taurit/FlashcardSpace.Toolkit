using System.Text.Json.Serialization;

namespace CoreLibrary.Services;

/// <summary>
/// A model of txt2img request body (or precisely, a subset of parameters I find useful) passed to stable-diffusion-webui API.
/// </summary>
record TextToImageRequestModel(
    [property: JsonPropertyName("prompt")] string Prompt,
    [property: JsonPropertyName("negative_prompt")] string NegativePrompt,

    // Remember that models are trained to generate certain width(s)/height(s) of images. Set it wrong, and you get garbage quality images!
    [property: JsonPropertyName("width")] int Width,
    [property: JsonPropertyName("height")] int Height,

    // Number of images to generate. Generating several images at one, reduces few seconds of overhead per image.
    [property: JsonPropertyName("batch_size")] int NumImages,

    // Number of steps to run the model. Higher number of steps can generate better quality images, but takes longer.
    // This is also something specific to a model you use! Some need 7, some 30 to get good results.
    [property: JsonPropertyName("num_steps")] int NumSteps,

    // CFG Scale: range is 1-30. The higher, the more model focuses on the prompt (at the cost of less creativity).
    [property: JsonPropertyName("cfg_scale")] int CfgScale,

    [property: JsonPropertyName("sampler_name")] string SamplerName,

    // Seed is a string that can be used to generate the same image again. -1 means random seed.
    [property: JsonPropertyName("seed")] int Seed,

    // API is stateful and has some global settings, notably: the model loaded to GPU!
    // This property allows to override it per-request.
    [property: JsonPropertyName("override_settings")] OverrideSettingsModel OverrideSettings,

    [property: JsonPropertyName("refiner_checkpoint")] string RefinerCheckpointId,

    // Refiner switcher at certain percentage of the process. Range is 0-1. Good typical values are 0.7-0.8.
    [property: JsonPropertyName("refiner_switch_at")] decimal RefinerSwitchAt

);

record OverrideSettingsModel(
    [property: JsonPropertyName("sd_model_checkpoint")] string StableDiffusionModelCheckpointId
    );
