namespace BookToAnki.Models;

public record SingleBookPart(string BookFolder, string AudioRelativePath)
{
    public string AudioAbsolutePath => Path.Combine(BookFolder, AudioRelativePath);
    public string OriginalTextPath => Path.ChangeExtension(AudioAbsolutePath, ".txt");
    public string GoogleTextToSpeechTranscriptPath => Path.ChangeExtension(AudioAbsolutePath, ".json");
}
