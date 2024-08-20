using FfmpegHelper.Models;
using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace FfmpegHelper.Services;

public static class FfmpegLauncher
{
    private const string PathToFfmpeg = "d:\\ProgramData\\Tools\\ffmpeg.exe";

    private static readonly Regex jsonExtractor =
        new(@"\[Parsed_loudnorm.*\].*(?<json>\{.*\})", RegexOptions.Compiled | RegexOptions.Singleline);

    public static async Task<string> StartFfmpegWithParameters(string parameters)
    {
        using var ffmpegProcess = new Process();
        ffmpegProcess.StartInfo.FileName = PathToFfmpeg;
        ffmpegProcess.StartInfo.Arguments = parameters;
        ffmpegProcess.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
        ffmpegProcess.StartInfo.UseShellExecute = false;
        ffmpegProcess.StartInfo.RedirectStandardOutput = true;
        ffmpegProcess.StartInfo.RedirectStandardError = true;
        ffmpegProcess.Start();
        await ffmpegProcess.WaitForExitAsync();
        var output = await ffmpegProcess.StandardOutput.ReadToEndAsync();
        var error = await ffmpegProcess.StandardError.ReadToEndAsync();
        return output + Environment.NewLine + error;
    }

    public static async Task<NormalizationParameters> GetAudioFileParameters(string rawFilePath, CancellationToken ct)
    {
        if (!File.Exists(rawFilePath))
            throw new ArgumentException($"The file {rawFilePath} does not exist, so you shouldn't be asking for its parameters.");

        var output = await StartFfmpegWithParameters(
            $"-i \"{rawFilePath}\" -af loudnorm=" +
            "print_format=json -f null -");
        return ParseAudioFileParametersFromFFmpegOutput(output);
    }

    public static NormalizationParameters ParseAudioFileParametersFromFFmpegOutput(string output)
    {
        var jsonPart = jsonExtractor.Matches(output).Single().Groups["json"].Value;
        var jsonParsed = JsonSerializer.Deserialize<NormalizationParameters>(jsonPart)!;
        return jsonParsed;
    }
}
