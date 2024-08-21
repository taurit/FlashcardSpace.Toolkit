using BookToAnki.Models;
using System.Diagnostics;

namespace BookToAnki.Services;

internal static class ExtractAudioSegmentHelper
{
    internal static async Task<string> ExtractAudioSegment(string inputPath, double startTimeInSeconds, double endTimeInSeconds,
        AudioShift? shift, string outputPath)
    {
        // There is a bias in times received from speech-to-text and 0.3 is a reasonable default to let the words sound to the end and not start too early.
        startTimeInSeconds += 0.3;
        endTimeInSeconds += 0.3;

        if (shift is not null)
        {
            startTimeInSeconds += shift.TimeShiftBeginning.TotalSeconds;
            endTimeInSeconds += shift.TimeShiftEnd.TotalSeconds;
        }

        if (startTimeInSeconds < 0)
        {
            throw new ArgumentException("Start time cannot be negative... If this happened in practice, think of some workaround, should be very rare.");
        }

        // Convert back to string format
        var startTime = $"{startTimeInSeconds}s".Replace(",", ".");
        var endTime = $"{endTimeInSeconds}s".Replace(",", ".");

        // Prepare the ffmpeg process
        var ffmpegProcess = new Process();
        ffmpegProcess.StartInfo.FileName = "ffmpeg";

        // i tried trimming silence, but it cuts a lot of proper parts of words and for now i cannot see a problem that it solves really,
        // -af \"silenceremove=start_periods=1:stop_periods=0:detection=peak:start_threshold=-30dB:stop_threshold=-30dB\"
        // I can make it less aggressive b changing 30dB to 50 dB if I want to experiment again

        ffmpegProcess.StartInfo.Arguments =
            $"-y -ss {startTime} -i \"{inputPath}\" -to {endTime} -map_metadata -1 -c copy -copyts \"{outputPath}\"";
        // compression with quality 2 or 3 gives bigger file still, and compression 7 our of 9 gives modest improvement, so it's not worth it.

        ffmpegProcess.StartInfo.UseShellExecute = false;
        ffmpegProcess.StartInfo.CreateNoWindow = true;

        // Run ffmpeg and wait for it to finish
        ffmpegProcess.Start();
        await ffmpegProcess.WaitForExitAsync();

        if (!File.Exists(outputPath) || new FileInfo(outputPath).Length == 0)
            throw new InvalidOperationException(
                "Error extracting sentence audio sample from the book: output file doesn't exist or has size 0");

        return outputPath;
    }
}
