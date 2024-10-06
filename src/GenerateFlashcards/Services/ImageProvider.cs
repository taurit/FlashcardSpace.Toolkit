using CoreLibrary.Models;
using CoreLibrary.Services;
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
        settings.ImageProviderCacheFolder.EnsureDirectoryExists();

        var notesWithImages = new List<FlashcardNote>();
        int noteIndex = 0;

        foreach (var note in notes)
        {
            noteIndex++;
            // log progress (note number, total number of notes, percentage)
            logger.LogInformation("Generating flashcard {NoteIndex}/{TotalNotes} ({Percentage}%): {Term}",
                noteIndex, notes.Count, noteIndex * 100 / notes.Count, note.Term);

            var images = await imageCandidatesGenerator.GenerateImageVariants(note.TermStandardizedFormEnglishTranslation, note.ContextEnglishTranslation);

            var imagesSavedToDisk = new List<string>();
            foreach (var image in images)
            {
                var imageFingerprint = image.Base64EncodedImage.GetHashCodeStable(15);
                // JPG format requires one-time setup: SD WebUI Settings -> File format for images -> change from default `png` to `jpeg`
                var imageFilePath = Path.Combine(settings.ImageProviderCacheFolder, $"{imageFingerprint}.jpg");

                await File.WriteAllBytesAsync(imageFilePath, Convert.FromBase64String(image.Base64EncodedImage));
                logger.LogDebug("Saved image to disk: {ImageFilePath}", imageFilePath);

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

