using CoreLibrary.Models;
using CoreLibrary.Services.GenerativeAiClients.StableDiffusion;
using CoreLibrary.Utilities;
using Microsoft.Extensions.Logging;

namespace GenerateFlashcards.Services;

internal record ImageCandidatesProviderSettings(string ImageProviderCacheFolder);

internal class ImageProvider(
    ImageCandidatesProviderSettings settings,
    ImageCandidatesGenerator imageCandidatesGenerator,

    ILogger<ImageProvider> logger
    )
{
    public async Task<List<FlashcardNote>> AddImageCandidates(List<FlashcardNote> notes)
    {
        if (Parameters.SkipImageGeneration)
        {
            logger.LogInformation("Skipping image generation as requested in `Parameters.cs`.");
            return notes;
        }

        settings.ImageProviderCacheFolder.EnsureDirectoryExists();

        var notesWithImages = new List<FlashcardNote>();
        int noteIndex = 0;

        foreach (var note in notes)
        {
            noteIndex++;
            // log progress (note number, total number of notes, percentage)
            logger.LogInformation("Generating images for note {NoteIndex}/{TotalNotes} ({Percentage}%): {Term} ({Sentence})",
                noteIndex, notes.Count, noteIndex * 100 / notes.Count, note.Term, note.ContextEnglishTranslation);

            var images = await imageCandidatesGenerator.GenerateImageVariants(note.TermStandardizedFormEnglishTranslation, note.ContextEnglishTranslation);

            var imagesSavedToDisk = new List<string>();
            foreach (var image in images)
            {
                var imageBytes = Convert.FromBase64String(image.Base64EncodedImage);
                var imageFingerprint = imageBytes.GetHashCodeStable(15);

                // By default API responds with PNG images.
                // To get JPG format, one-time setup is required:
                // SD WebUI Settings -> File format for images -> change from default `png` to `jpeg`
                var imageFilePath = Path.Combine(settings.ImageProviderCacheFolder, $"{imageFingerprint}.jpg");
                if (!File.Exists(imageFilePath))
                {
                    await File.WriteAllBytesAsync(imageFilePath, imageBytes);
                    logger.LogDebug("Saved image to disk: {ImageFilePath}", imageFilePath);
                }
                else
                {
                    logger.LogDebug("Image already exists on disk: {ImageFilePath}", imageFilePath);
                }

                imagesSavedToDisk.Add(imageFilePath);
            }

            var noteWithImages = note with
            {
                ImageCandidates = imagesSavedToDisk
            };
            notesWithImages.Add(noteWithImages);
        }
        return notesWithImages;
    }

}

