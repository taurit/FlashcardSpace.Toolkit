using BookToAnki.Models;
using BookToAnki.NotePropertiesDatabase;

namespace BookToAnki.Services;

public class AudioSampleSelector
{
    private readonly NoteProperties _noteProperties;

    public AudioSampleSelector(NoteProperties noteProperties)
    {
        _noteProperties = noteProperties;
    }

    public SentenceWithSound? TrySelectBestAudioSample(WordUsageExample wordUsage)
    {
        if (wordUsage.TranscriptMatches is null || !wordUsage.TranscriptMatches.Any())
            throw new InvalidOperationException("Generating notes with no audio samples is not supported.");

        var preferredAudioSample = _noteProperties.GetAudioSample(new PrefKey(wordUsage.Word, wordUsage.Sentence.Text));
        if (preferredAudioSample is not null)
        {
            // user selected preferred sample explicitly
            var sentence = wordUsage.TranscriptMatches.Distinct().SingleOrDefault(x => x.Matches(preferredAudioSample));
            if (sentence is not null) return sentence;
        }

        var sorted = SortByBestPromising(wordUsage.TranscriptMatches);
        var likelyBestTranscriptionMatch = sorted.First();
        return likelyBestTranscriptionMatch;

    }

    private List<SentenceWithSound> SortByBestPromising(List<SentenceWithSound> samples)
    {
        return samples
            .OrderBy(x => GetAudioFilePriority(x.PathToAudioFile))

            // just to make this list predictably sorted
            .ThenBy(x => x.PathToAudioFile)
            .ThenBy(x => x.WordsFromTranscript.First().StartTimeSeconds)
            .ToList()
            ;

    }

    int GetAudioFilePriority(string pathToAudioFile)
    {
        return pathToAudioFile switch
        {
            var path when path.Contains("hp_01") => 0, // preferred - great quality, least spoilers
            var path when path.Contains("hp_02") => 0, // still great quality
            var path when path.Contains("hp_03") => 0, // still great quality
            var path when path.Contains("hp_07") => 1, // great quality, although spoilers more likely
            _ => 2 // books 4+ are in worse audio quality and from different voice actor
        };
    }
}
