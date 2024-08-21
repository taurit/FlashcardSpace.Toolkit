using BookToAnki.Models;
using BookToAnki.Models.AzureTranscripts;
using BookToAnki.Models.GoogleCloudTranscripts;
using System.Text.Json;

namespace BookToAnki.Services;

public static class AzureTranscriptReader
{
    public static Transcript ReadTranscript(string transcriptFileName, string audioFilePath)
    {
        var content = File.ReadAllText(transcriptFileName);
        var transcript = JsonSerializer.Deserialize<AzureTranscriptJsonModel>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        });

        // flatten, and only keep alternatives with maximum confidence
        var words = transcript.RecognizedPhrases.Where(x => x.Channel == 0).SelectMany(x => x.NBest.Take(1)).SelectMany(x => x.Words).ToList();

        // hack: convert to a simpler Google Cloud format for compatibility to avoid cloning similar models
        // Google Cloud was supported first and application knows how to work with such models

        var convertedWords = words.Select(ConvertAzureTranscriptWordToGoogleCloudTranscriptWord).ToList();

        return new Transcript(convertedWords, audioFilePath);
    }

    private static GoogleCloudTranscriptWord ConvertAzureTranscriptWordToGoogleCloudTranscriptWord(AzureTranscriptWord aw)
    {
        var offset = TimeSpan.FromTicks((long)aw.OffsetInTicks);
        var duration = TimeSpan.FromTicks((long)aw.DurationInTicks);

        var offsetInSeconds = offset.TotalSeconds;
        var durationInSeconds = duration.TotalSeconds;

        var endTimeInSeconds = offsetInSeconds + durationInSeconds;

        return new GoogleCloudTranscriptWord(aw.Word, offsetInSeconds, endTimeInSeconds);
    }
}
