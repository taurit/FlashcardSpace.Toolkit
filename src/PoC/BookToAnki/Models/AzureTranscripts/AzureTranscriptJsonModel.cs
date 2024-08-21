namespace BookToAnki.Models.AzureTranscripts;

record AzureTranscriptJsonModel(AzureTranscriptRecognizedPhrase[] RecognizedPhrases);
internal record AzureTranscriptRecognizedPhrase(int Channel, AzureTranscriptNBestPhrases[] NBest);

internal record AzureTranscriptNBestPhrases(AzureTranscriptWord[] Words);

internal record AzureTranscriptWord(string Word, string Offset, string Duration, decimal OffsetInTicks, decimal DurationInTicks);
