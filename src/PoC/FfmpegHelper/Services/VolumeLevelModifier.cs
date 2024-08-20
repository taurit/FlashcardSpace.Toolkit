using System.Diagnostics;

namespace FfmpegHelper.Services;

public class VolumeLevelModifier
{
    // this program takes file path as input and produces parameters ready to use by ffmpeg as output
    private const string ffmpegLoudnormHelperPath = @"d:\ProgramData\Tools\ffmpeg-lh.exe";

    public async Task NormalizeVolumeLevel(string inputFile, string outputFile, bool disallowFurtherPadding = false)
    {
        var loudnormFilterParameters = await GetParametersForVolumeNormalization(inputFile);

        var ffdshowArgs = $" -i \"{inputFile}\" {loudnormFilterParameters}" +
                          // normalize volume

                          //" -af \"loudnorm=..., " +
                          // then trim silence
                          //    " silenceremove=start_periods=1" +
                          //    ":start_duration=0.0s" + // ile ciszy pozostawić na początku? chyba jednak nie, zachowuje się jak "ile obciąć zawsze"
                          //    ":start_threshold=-50dB" +
                          //    ":stop_periods=0" +
                          //    ":stop_duration=0.0s" + // chyba nie: ile ciszy pozostawić na końcu - żeby jakiś dźwięk wybrzmiał. Na oko w Audacity, wybrzmiewa ok. 0,3 sekundy, a więcej nas w sumie nie boli // 
                          //    ":stop_threshold=-50dB" +
                          //    "\" " +
                          " -codec:a libmp3lame -b:a 160k " +
                          " -y" +
                          $" \"{outputFile}\"";

        ffdshowArgs = ffdshowArgs.Replace("print_format=summary", "print_format=json");

        var normalizationProcessOutput = await FfmpegLauncher.StartFfmpegWithParameters(ffdshowArgs);
        var paramsAfterNormalization = FfmpegLauncher.ParseAudioFileParametersFromFFmpegOutput(normalizationProcessOutput);

        var offsetToTarget = double.Parse(paramsAfterNormalization.target_offset.Replace(".", ","));
        if (offsetToTarget > 2 && !disallowFurtherPadding)
        {
            // loudnorm doesn't add gain to short samples, we need a workaround:
            // 1) make the audio a bit longer
            // 2) normalize
            // 3) trim after normalization - I skip it to avoid having to calculate length of audio, which would require another process start. Doesn't matter if rare 1s sample will become 3s.
            // https://superuser.com/questions/1281327/ffmpeg-loudnorm-filter-does-not-make-audio-louder
            // http://underpop.online.fr/f/ffmpeg/help/apad.htm.gz

            Console.WriteLine("2nd attempt, now with padding!");

            var paddedInputFilePath = Path.GetTempFileName() + ".mp3";
            var ffdshowArgsToPad = $" -i \"{inputFile}\" -codec:a libmp3lame -af apad=pad_dur=2.5 -b:a 160k -y \"{paddedInputFilePath}\"";

            await FfmpegLauncher.StartFfmpegWithParameters(ffdshowArgsToPad);

            // 2nd attempt, but this time with padded input file - I assume this is the case where audio sample duration has lt 1s
            await NormalizeVolumeLevel(paddedInputFilePath, outputFile, true);
        }

        Console.WriteLine(normalizationProcessOutput);
    }

    private static async Task<string> GetParametersForVolumeNormalization(string inputFile)
    {
        using var ffmpegLhProcess = new Process();
        ffmpegLhProcess.StartInfo.FileName = ffmpegLoudnormHelperPath;

        // I set to 20 to match the average loudness of other mp3s in Anki collection. The default was -16, which could result in too
        // loud output after normalization.
        var integratedLoudnessTarget = "-20.0";

        // I am changing to -4 from the default -1 to match what is already in samples that I like. I'm not sure if it's good idea yet.
        var maximumTruePeak = "-4.0";

        ffmpegLhProcess.StartInfo.Arguments = $"--i {integratedLoudnessTarget} --tp {maximumTruePeak} \"{inputFile}\"";
        ffmpegLhProcess.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
        ffmpegLhProcess.StartInfo.UseShellExecute = false;
        ffmpegLhProcess.StartInfo.RedirectStandardOutput = true;
        ffmpegLhProcess.StartInfo.RedirectStandardError = true;
        ffmpegLhProcess.Start();
        await ffmpegLhProcess.WaitForExitAsync();
        var ffmpegParameters = await ffmpegLhProcess.StandardOutput.ReadToEndAsync();

        if (String.IsNullOrWhiteSpace(ffmpegParameters))
        {
            var err = await ffmpegLhProcess.StandardError.ReadToEndAsync();
            throw new Exception($"ffmpeg-lh.exe failed to provide params. Error output: {err}");
        }

        return ffmpegParameters;
    }
}
