using BookToAnki.Models.GoogleCloudTranscripts;
using MemoryPack;

namespace BookToAnki.Models;

[MemoryPackable]
public partial record Transcript(List<GoogleCloudTranscriptWord> TranscriptWords, string AudioFilePath);
