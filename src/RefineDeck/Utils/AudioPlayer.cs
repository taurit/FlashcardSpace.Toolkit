using NAudio.Wave;

namespace RefineDeck.Utils;
internal static class AudioPlayer
{
    private static IWavePlayer? _waveOut;
    private static AudioFileReader? _audioFileReader;

    internal static void PlayAudio(string filePath)
    {
        _waveOut = new WaveOutEvent();
        _audioFileReader = new AudioFileReader(filePath);
        _waveOut.Init(_audioFileReader);
        _waveOut.Play();
    }
}
