using BookToAnki.Models.GoogleCloudTranscripts;
using BookToAnki.Services;
using CoreLibrary.Interfaces;
using System.Diagnostics;

namespace BookToAnki.Models;

/// <param name="Sentence"></param>
/// <param name="WordsFromTranscript"></param>
/// <param name="PathToAudioFile">A Full audio file (whole book or chapter). NOT a single sentence.</param>
[DebuggerDisplay("{Sentence.Text} in  {PathToAudioFile}")]
public record SentenceWithSound(Sentence Sentence, List<GoogleCloudTranscriptWord> WordsFromTranscript, string PathToAudioFile)
{
    public bool Matches(AudioSample sample) =>
        PathToAudioFile == sample.PathToAudioFile &&
        WordsFromTranscript.First().StartTimeSeconds == sample.FirstWordStartTime &&
        WordsFromTranscript.Last().EndTimeSeconds == sample.LastWordEndTime;
}
