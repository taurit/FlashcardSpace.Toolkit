using BookToAnki.Models;
using MemoryPack;

namespace BookToAnki.Services;

enum TranscriptFlavor { GoogleCloud, Azure }

public class TranscriptReader
{
    public static Transcript ReadTranscript(string transcriptFileName, string audioFilePath)
    {
        var processedTranscriptCacheFileName = $"{transcriptFileName}.cache.bin";

        if (File.Exists(processedTranscriptCacheFileName))
        {
            var cacheContent = File.ReadAllBytes(processedTranscriptCacheFileName);
            var transcriptFromCache = MemoryPackSerializer.Deserialize<Transcript>(cacheContent);
            if (transcriptFromCache is null)
            {
                throw new FileLoadException($"Cache file `{processedTranscriptCacheFileName}` seems broken, remove it!");
            }
            return transcriptFromCache;
        }

        var content = File.ReadAllText(transcriptFileName);
        var transcriptFlavor = content.Contains("\"alternatives\":") ? TranscriptFlavor.GoogleCloud : TranscriptFlavor.Azure;

        Transcript? transcript = null;
        if (transcriptFlavor == TranscriptFlavor.GoogleCloud)
        {
            transcript = GoogleCloudTranscriptReader.ReadTranscript(transcriptFileName, audioFilePath);
        }
        else
        {
            transcript = AzureTranscriptReader.ReadTranscript(transcriptFileName, audioFilePath);
        }

        var bin = MemoryPackSerializer.Serialize(transcript);
        File.WriteAllBytes(processedTranscriptCacheFileName, bin);

        return transcript;
    }
}
