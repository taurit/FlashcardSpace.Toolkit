using BookToAnki.Models.GoogleCloudTranscripts;
using BookToAnki.Services;
using System.Diagnostics;
using AdvancedSentenceExtractor.Models;

namespace BookToAnki.Models;

/// <param name="Sentence"></param>
/// <param name="WordsFromTranscript"></param>
/// <param name="PathToAudioFile">A Full audio file (whole book or chapter). NOT a single sentence.</param>
[DebuggerDisplay("{Sentence.Text} in  {PathToAudioFile}")]
public record SentenceWithSound(Sentence Sentence, List<GoogleCloudTranscriptWord> WordsFromTranscript, string PathToAudioFile)
{
    public bool Matches(AudioSample sample) =>
        this.PathToAudioFile == sample.PathToAudioFile &&
        this.WordsFromTranscript.First().StartTimeSeconds == sample.FirstWordStartTime &&
        this.WordsFromTranscript.Last().EndTimeSeconds == sample.LastWordEndTime;
}
