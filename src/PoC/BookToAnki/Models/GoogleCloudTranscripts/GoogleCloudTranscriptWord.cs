using MemoryPack;

namespace BookToAnki.Models.GoogleCloudTranscripts;

public record GoogleCloudTranscriptWordInput(String? Word, string StartTime, string EndTime);

[MemoryPackable]
public partial record GoogleCloudTranscriptWord(String Word, double StartTimeSeconds, double EndTimeSeconds);
