using BookToAnki.Models;

namespace BookToAnki.Services;

public record AudioSample(string PathToAudioFile, double FirstWordStartTime, double LastWordEndTime)
{
    public static AudioSample FromSentenceWithSound(SentenceWithSound sentence)
    {
        var firstWordStartTime = sentence.WordsFromTranscript.First().StartTimeSeconds;
        var lastWordEndTime = sentence.WordsFromTranscript.Last().EndTimeSeconds;
        return new AudioSample(sentence.PathToAudioFile, firstWordStartTime, lastWordEndTime);
    }
}
