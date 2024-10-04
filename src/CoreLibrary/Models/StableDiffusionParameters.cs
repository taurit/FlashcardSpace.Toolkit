using CoreLibrary.Utilities;
using System.Text.RegularExpressions;

namespace CoreLibrary.Models;

public record StableDiffusionParameters(string Prompt, string NegativePrompt, int Steps, string Sampler, string ScheduleType, int CfgScale, string Seed, string FaceRestoration, int Width, int Height, string ModelHash, string Model, string Rng, string Refiner, string RefinerSwitchAt, string Version)
{
    private static readonly Regex ParamsRegex = new Regex(
        @"^
    (?<prompt>.*?)\s*
    Negative\ prompt:\ (?<negativePrompt>.*?)\s*
    Steps:\ (?<steps>\d+),\s*
    Sampler:\ (?<sampler>.*?),\s*
    Schedule\ type:\ (?<scheduleType>.*?),\s*
    CFG\ scale:\ (?<cfgScaleInt>[\d]+)[\.,]\d+,\s*
    Seed:\ (?<seed>.*?),\s*
    Face\ restoration:\ (?<faceRestoration>.*?),\s*
    Size:\ (?<width>\d+)x(?<height>\d+),\s*
    Model\ hash:\ (?<modelHash>.*?),\s*
    Model:\ (?<model>.*?),\s*
    RNG:\ (?<rng>.*?),\s*
    Refiner:\ (?<refiner>.*?),\s*
    Refiner\ switch\ at:\ (?<refinerSwitchAt>.*?),\s*
    Version:\ (?<version>.*)
    $",
        RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace);


    public static StableDiffusionParameters? FromString(string userCommentTagValue)
    {
        if (String.IsNullOrWhiteSpace(userCommentTagValue)) return null;

        var match = ParamsRegex.Match(userCommentTagValue);
        if (match.Success)
        {
            return new StableDiffusionParameters(
                match.Groups["prompt"].Value.Trim(),
                match.Groups["negativePrompt"].Value.Trim(),
                int.Parse(match.Groups["steps"].Value),
                match.Groups["sampler"].Value,
                match.Groups["scheduleType"].Value,
                int.Parse(match.Groups["cfgScaleInt"].Value),
                match.Groups["seed"].Value,
                match.Groups["faceRestoration"].Value,
                int.Parse(match.Groups["width"].Value),
                int.Parse(match.Groups["height"].Value),
                match.Groups["modelHash"].Value,
                match.Groups["model"].Value,
                match.Groups["rng"].Value,
                match.Groups["refiner"].Value,
                match.Groups["refinerSwitchAt"].Value,
                match.Groups["version"].Value
            );
        }
        return null;
    }


    public string PromptWithoutStyleKeywords =>
        StableDiffusionKeywordRemover.RemoveStyleKeywordsFromPrompt(Prompt);
}
