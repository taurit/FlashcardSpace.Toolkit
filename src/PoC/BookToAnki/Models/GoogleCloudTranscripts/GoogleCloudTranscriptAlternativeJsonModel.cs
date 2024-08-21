using System.Diagnostics.CodeAnalysis;

namespace BookToAnki.Models.GoogleCloudTranscripts;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Local", Justification = "Instantiated in deserialization")]
record GoogleCloudTranscriptAlternativeJsonModel(decimal Confidence, String Transcript, GoogleCloudTranscriptWordInput[] Words);
