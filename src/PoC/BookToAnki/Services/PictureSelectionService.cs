namespace BookToAnki.Services;

public record ImageCandidate(string FullPath, bool IsSelected);

public class PictureSelectionService(string imagesRepositoryFolder)
{
    public List<ImageCandidate> GetImageCandidates(string word, string sentence)
    {
        string wordFolder = Path.Combine(imagesRepositoryFolder, word).ToLowerInvariant();
        List<ImageCandidate> imageList = new List<ImageCandidate>(5);

        if (Directory.Exists(wordFolder))
        {
            var preferredImageFileName = TryGetPreferredImageFileName(wordFolder);

            foreach (var file in Directory.GetFiles(wordFolder))
            {
                var lower = file.ToLower();
                if (lower.EndsWith(".100.webp") || lower.Contains("dalle"))
                {
                    bool isExplicitlySelected = file == preferredImageFileName;
                    imageList.Add(new ImageCandidate(file, isExplicitlySelected));
                }
            }
        }
        return imageList;
    }


    private void SavePreferredImageFileName(string? directory, string preferredFileFullPath)
    {
        if (directory is null) return;
        var preferenceFile = Path.Combine(directory, "preference.txt");
        var preferredImageFileName = Path.GetFileName(preferredFileFullPath);
        File.WriteAllText(preferenceFile, preferredImageFileName);
    }

    private string? TryGetPreferredImageFileName(string directory)
    {
        var preferenceFile = Path.Combine(directory, "preference.txt");
        if (!File.Exists(preferenceFile)) return null;
        var fileContent = File.ReadAllText(preferenceFile).Trim(); // expected to contain file name of preferred image
        var preferredFilePath = Path.Combine(directory, fileContent);
        return preferredFilePath;
    }


    public void ChooseImage(string selectedImageFileName)
    {

    }
}
