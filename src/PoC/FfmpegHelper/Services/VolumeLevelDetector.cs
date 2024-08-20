using System.Globalization;

namespace FfmpegHelper.Services;

public class VolumeLevelDetector
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="audioFileName"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task<bool> IsTooQuiet(string audioFileName, CancellationToken ct)
    {
     /* Thresholds determined heuristically:
        FileName                         | o_i  |olra|o_thr |o_tp |in_i  |ilra|i_thr |i_tp
        .\TestData\Fine\rec1496218116.mp3;-24.02;0.00;-34.02;-8.73;-21.48;0.00;-31.48;-6.21
        .\TestData\Fine\rec1496218136.mp3;-24.00;0.00;-34.00;-7.62;-21.20;0.00;-31.20;-4.82
        .\TestData\Fine\rec1496218572.mp3;-24.00;0.00;-34.47;-9.48;-17.88;0.00;-28.35;-3.36
        .\TestData\Fine\rec1496218582.mp3;-24.01;0.00;-34.47;-7.93;-19.63;0.00;-30.08;-3.56
        .\TestData\Fine\rec1496218586.mp3;-24.01;0.00;-34.01;-8.53;-21.42;0.00;-31.42;-5.95
        .\TestData\Fine\rec1496218596.mp3;-22.96;0.20;-33.71;-4.66;-20.28;1.30;-31.19;-1.75
        .\TestData\Fine\rec1496218603.mp3;-24.00;0.00;-34.64;-7.43;-20.32;0.00;-30.96;-3.75
                                                                   !!!!!!      !!!!!! !!!!!
        .\TestData\TooQuiet\rec148032.mp3;-23.56;2.80;-34.54;-4.32;-29.03;2.00;-40.08;-10.39
        .\TestData\TooQuiet\rec148033.mp3;-23.48;1.70;-34.62;-4.53;-27.47;0.90;-38.69;-9.01
        .\TestData\TooQuiet\rec148763.mp3;-23.56;0.00;-33.71;-7.04;-43.72;0.40;-53.94;-27.18
        .\TestData\TooQuiet\rec148763.mp3;-23.93;0.00;-34.19;-6.63;-43.58;1.50;-53.93;-25.86
        .\TestData\TooQuiet\rec148767.mp3;-23.58;0.00;-33.58;-6.40;-45.33;0.00;-55.40;-28.08
      *
      * Intuitively it makes sense - audio that is too quiet has lower loudness measure, lower-situated threshold and lower-situated peak.
      */
        var audioFileParameters = await FfmpegLauncher.GetAudioFileParameters(audioFileName, ct);
        
        //Console.WriteLine(audioFileParameters);

        var integratedLoudnessAsString = audioFileParameters.input_i.Replace(".", ",");

        if (integratedLoudnessAsString == "-inf")
        {
            // based on samples I tested, such file is probably broken somehow (or just plain silence) - so not loud enough, but
            // processing it won't help
            return false;
        }

        var integratedLoudness = Double.Parse(integratedLoudnessAsString, new CultureInfo("pl-PL"));
        var integratedLoudnessTargetSeemsTooLow = integratedLoudness < -25;
        bool tooQuiet = integratedLoudnessTargetSeemsTooLow;

        return tooQuiet;
    }
}