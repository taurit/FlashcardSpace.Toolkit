using System.Diagnostics;

namespace FfmpegHelper.Services;

public class DurationDetector
{
    public async Task<decimal> GetAudioSampleDurationInSeconds(string fileName)
    {
        var ffProbeOutput = await StartFfprobeReadDuration($"-v error -show_entries format=duration -of csv=p=0 \"{fileName}\"");
        var durationInSeconds = decimal.Parse(ffProbeOutput.Trim().Replace(".", ","));
        return durationInSeconds;
    }

    private static async Task<string> StartFfprobeReadDuration(string parameters)
    {
        using var ffmpegProcess = new Process();
        ffmpegProcess.StartInfo.FileName = @"D:\ProgramData\Tools\ffmpeg\bin\ffprobe.exe";
        ffmpegProcess.StartInfo.Arguments = parameters;
        ffmpegProcess.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
        ffmpegProcess.StartInfo.UseShellExecute = false;
        ffmpegProcess.StartInfo.RedirectStandardOutput = true;
        ffmpegProcess.StartInfo.RedirectStandardError = false;
        ffmpegProcess.Start();
        await ffmpegProcess.WaitForExitAsync();
        var output = await ffmpegProcess.StandardOutput.ReadToEndAsync();
        return output;
    }
}