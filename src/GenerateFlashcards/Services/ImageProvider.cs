using CoreLibrary.Models;
using CoreLibrary.Services;
using CoreLibrary.Utilities;

namespace GenerateFlashcards.Services;

internal record ImageCandidatesProviderSettings(string ImageProviderCacheFolder);

internal class ImageProvider(
    ImageCandidatesProviderSettings settings,
    ImageCandidatesGenerator imageCandidatesGenerator
    )
{
    public async Task<List<FlashcardNote>> AddImageCandidates(List<FlashcardNote> notes)
    {
        settings.ImageProviderCacheFolder.EnsureDirectoryExists();

        var notesWithImages = new List<FlashcardNote>();
        foreach (var note in notes)
        {
            var images = await imageCandidatesGenerator.GenerateImageVariants(
                    note.TermStandardizedFormEnglishTranslation, note.ContextEnglishTranslation, 2, 2);

            var imagesSavedToDisk = new List<string>();
            foreach (var image in images)
            {
                var imageFingerprint = image.Base64EncodedImage.GetHashCodeStable(15);
                // JPG format requires one-time setup: SD WebUI Settings -> File format for images -> change from default `png` to `jpeg`
                var imageFilePath = Path.Combine(settings.ImageProviderCacheFolder, $"{imageFingerprint}.jpg");

                await File.WriteAllBytesAsync(imageFilePath, Convert.FromBase64String(image.Base64EncodedImage));
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

