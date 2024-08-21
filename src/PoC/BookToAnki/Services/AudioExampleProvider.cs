using BookToAnki.Models;

namespace BookToAnki.Services;
public class AudioExampleProvider
{
    private readonly string _audioCacheFolderPath;

    public AudioExampleProvider(string audioCacheFolderPath)
    {
        _audioCacheFolderPath = audioCacheFolderPath;
    }

    /// <returns>
    /// File name only, not full path!
    /// </returns>
    public async Task<string> GenerateAudioSample(SentenceWithSound sentence, AudioShift? audioShift = null)
    {
        var outputFileName = CreateDeterministicValidFileName(sentence.Sentence.Text, audioShift);
        var outputPath = Path.Combine(_audioCacheFolderPath, outputFileName);
        if (File.Exists(outputPath) && new FileInfo(outputPath).Length > 0) return outputFileName; // already in cache

        var audioStartTime = sentence.WordsFromTranscript.First().StartTimeSeconds;
        var audioEndTime = sentence.WordsFromTranscript.Last().EndTimeSeconds;

        await ExtractAudioSegmentHelper.ExtractAudioSegment(sentence.PathToAudioFile, audioStartTime, audioEndTime, audioShift, outputPath);

        return outputFileName;
    }

    readonly char[] _allowedPunctuation = { '!' };

    private string CreateDeterministicValidFileName(string sentence, AudioShift? shift)
    {
        // List of invalid characters for file names
        var invalidFileNameChars = Path.GetInvalidFileNameChars().ToList();

        // I want different audio samples for "- Harry?" and "- HARRY!". Thus, discerning question marks is important.
        sentence = sentence.Replace("?", "Q");

        // Replace whitespace and other invalid characters
        var validFileName = new string(sentence
            .Where(c => !invalidFileNameChars.Contains(c) && (!char.IsPunctuation(c) || _allowedPunctuation.Contains(c)))
            .Select(c => char.IsWhiteSpace(c) ? '_' : c)
            .Take(56) // limit length to avoid exceeding max
            .ToArray());

        validFileName = validFileName.Trim('_');

        // Append the padding if needed
        if (shift is not null)
        {
            validFileName += $".b{shift.TimeShiftBeginning.Ticks}.e{shift.TimeShiftEnd.Ticks}";
        }

        // Append the .mp3 extension
        return validFileName + ".mp3";
    }
}
