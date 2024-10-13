using System.Globalization;
using System.Text.RegularExpressions;

namespace CoreLibrary.Models;


public record StableDiffusionParameters(string Prompt, string NegativePrompt, int Steps, string Sampler, string ScheduleType, decimal CfgScale, Int64 Seed, string FaceRestoration, int Width, int Height, string ModelHash, string Model, string? Rng, string? Refiner, string? RefinerSwitchAt, string? Version, string? Fp8WeightMatch, string? CacheFp16WeightForLoraMatch)
{
    // First params seems to always be there, and be in the same order, so I try to parse them with 1 regex for simplicity:
    private static readonly Regex BasicParamsRegex = new Regex(
        @"^
    (?<prompt>.*?)\s*
    Negative\ prompt:\ (?<negativePrompt>.*?)\s*
    Steps:\ (?<steps>\d+),\s*
    Sampler:\ (?<sampler>.*?),\s*
    Schedule\ type:\ (?<scheduleType>.*?),\s*
    CFG\ scale:\ (?<cfgScaleDecimal>[\d]+([\.,]\d+)?),\s*
    Seed:\ (?<seed>.*?),\s*
    ",
        RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace);

    // Some other params might or might not be present, so I have individual regexes to detect them
    private static readonly Regex RefinerRegex = new Regex(@"Refiner:\ (?<refiner>.*?),\s*", RegexOptions.Compiled);
    private static readonly Regex RefinerSwitchAtRegex = new Regex(@"Refiner\ switch\ at:\ (?<refinerSwitchAt>.*?),\s*", RegexOptions.Compiled);
    private static readonly Regex VersionRegex = new Regex(@"Version:\ (?<version>.*?)$", RegexOptions.Compiled);
    private static readonly Regex Fp8WeightRegex = new Regex(@"FP8\ weight:\ (?<fp8Weight>.*?),\s*", RegexOptions.Compiled);
    private static readonly Regex CacheFp16WeightForLoraRegex = new Regex(@"Cache\ FP16\ weight\ for\ LoRA:\ (?<cacheFp16WeightForLora>.*?),\s*", RegexOptions.Compiled);
    private static readonly Regex RngRegex = new Regex(@"RNG:\ (?<rng>.*?),\s*", RegexOptions.Compiled);
    private static readonly Regex FaceRestorationRegex = new Regex(@"Face\ restoration:\ (?<faceRestoration>.*?),\s*", RegexOptions.Compiled);
    private static readonly Regex SizeRegex = new Regex(@"Size:\ (?<width>\d+)x(?<height>\d+),\s*", RegexOptions.Compiled);
    private static readonly Regex ModelHashRegex = new Regex(@"Model\ hash:\ (?<modelHash>.*?),\s*", RegexOptions.Compiled);
    private static readonly Regex ModelRegex = new Regex(@"Model:\ (?<model>.*?),\s*", RegexOptions.Compiled);

    // Example input:
    // --------
    // cat portrait, 
    // Negative prompt: lowres,bad anatomy,bad hands,text,error,missing fingers,extra digit,fewer digits,cropped,worst quality,low quality,jpeg artifacts,signature,watermark,username,blurry,nsfw,painting,drawing,illustration,cartoon,anime,sketch
    // Steps: 13, Sampler: DPM++ 2M, Schedule type: Karras, CFG scale: 4, Seed: 2535254266, Face restoration: CodeFormer, Size: 1216x832, Model hash: 31e35c80fc, Model: sd_xl_base_1.0, FP8 weight: Enable for SDXL, Cache FP16 weight for LoRA: True, Version: v1.10.1
    public static StableDiffusionParameters? FromString(string userCommentTagValue)
    {
        if (String.IsNullOrWhiteSpace(userCommentTagValue)) return null;

        var match = BasicParamsRegex.Match(userCommentTagValue);
        if (match.Success)
        {
            var response = new StableDiffusionParameters(
                match.Groups["prompt"].Value.Trim(),
                match.Groups["negativePrompt"].Value.Trim(),
                int.Parse(match.Groups["steps"].Value),
                match.Groups["sampler"].Value,
                match.Groups["scheduleType"].Value,
                // parse assuming format is either int like "4" or decimal with dot "4.5"
                decimal.Parse(match.Groups["cfgScaleDecimal"].Value, CultureInfo.InvariantCulture),
                Int64.Parse(match.Groups["seed"].Value),
                FaceRestoration: null,
                Width: 0,
                Height: 0,
                ModelHash: null,
                Model: null,
                Rng: null,
                Refiner: null,
                RefinerSwitchAt: null,
                Version: null,
                Fp8WeightMatch: null,
                CacheFp16WeightForLoraMatch: null
            );

            // try match other, non-guaranteed params too
            var refinerMatch = RefinerRegex.Match(userCommentTagValue);
            if (refinerMatch.Success)
                response = response with { Refiner = refinerMatch.Groups["refiner"].Value };

            var refinerSwitchAtMatch = RefinerSwitchAtRegex.Match(userCommentTagValue);
            if (refinerSwitchAtMatch.Success)
                response = response with { RefinerSwitchAt = refinerSwitchAtMatch.Groups["refinerSwitchAt"].Value };

            var versionMatch = VersionRegex.Match(userCommentTagValue);
            if (versionMatch.Success)
                response = response with { Version = versionMatch.Groups["version"].Value };

            var fp8WeightMatch = Fp8WeightRegex.Match(userCommentTagValue);
            if (fp8WeightMatch.Success)
                response = response with { Fp8WeightMatch = fp8WeightMatch.Groups["fp8Weight"].Value };

            var cacheFp16WeightForLoraMatch = CacheFp16WeightForLoraRegex.Match(userCommentTagValue);
            if (cacheFp16WeightForLoraMatch.Success)
                response = response with { CacheFp16WeightForLoraMatch = cacheFp16WeightForLoraMatch.Groups["cacheFp16WeightForLora"].Value };

            var rngMatch = RngRegex.Match(userCommentTagValue);
            if (rngMatch.Success)
                response = response with { Rng = rngMatch.Groups["rng"].Value };

            var faceRestorationMatch = FaceRestorationRegex.Match(userCommentTagValue);
            if (faceRestorationMatch.Success)
                response = response with { FaceRestoration = faceRestorationMatch.Groups["faceRestoration"].Value };

            var sizeMatch = SizeRegex.Match(userCommentTagValue);
            if (sizeMatch.Success)
            {
                response = response with
                {
                    Width = int.Parse(sizeMatch.Groups["width"].Value),
                    Height = int.Parse(sizeMatch.Groups["height"].Value)
                };
            }

            var modelHashMatch = ModelHashRegex.Match(userCommentTagValue);
            if (modelHashMatch.Success)
                response = response with { ModelHash = modelHashMatch.Groups["modelHash"].Value };

            var modelMatch = ModelRegex.Match(userCommentTagValue);
            if (modelMatch.Success)
                response = response with { Model = modelMatch.Groups["model"].Value };

            return response;
        }
        return null;
    }

}
