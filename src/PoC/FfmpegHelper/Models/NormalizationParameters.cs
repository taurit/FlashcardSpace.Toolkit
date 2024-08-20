using System.Diagnostics.CodeAnalysis;

namespace FfmpegHelper.Models;

/// <summary>
/// Docs: https://ffmpeg.org/ffmpeg-filters.html#loudnorm
/// </summary>
#pragma warning disable CS8618
[SuppressMessage("ReSharper", "InconsistentNaming")]
public record NormalizationParameters
{
    public string input_i { get; set; }
    public string input_tp { get; set; }
    public string input_lra { get; set; }
    public string input_thresh { get; set; }
    public string output_i { get; set; }
    public string output_tp { get; set; }
    public string output_lra { get; set; }
    public string output_thresh { get; set; }
    public string normalization_type { get; set; }
    public string target_offset { get; set; }
}