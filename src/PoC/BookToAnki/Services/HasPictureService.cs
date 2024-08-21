namespace BookToAnki.Services;
public class HasPictureService : IDisposable
{
    private readonly WordsLinker _wordsLinker;
    private readonly string _imagesRepositoryFolder;
    private HashSet<string> _subfolderListCache;
    private readonly FileSystemWatcher _watcher;

    public HasPictureService(WordsLinker wordsLinker, string imagesRepositoryFolder)
    {
        _wordsLinker = wordsLinker;
        _imagesRepositoryFolder = imagesRepositoryFolder;
        UpdateFoldersCache();

        // Initialize a new FileSystemWatcher and set its properties.
        _watcher = new FileSystemWatcher(imagesRepositoryFolder);

        // Watch for changes in LastAccess and LastWrite times, and the renaming of files or directories.
        _watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;

        // Only watch directories.
        _watcher.Filter = ""; // Empty filter for watching all files
        _watcher.IncludeSubdirectories = false; // Set to true if you need to monitor subdirectories

        // Add event handlers.
        _watcher.Created += OnCreated;
        _watcher.Deleted += OnDeleted;
        _watcher.Renamed += OnRenamed;

        // Begin watching.
        _watcher.EnableRaisingEvents = true;
    }

    private void UpdateFoldersCache()
    {
        this._subfolderListCache = Directory
            .EnumerateDirectories(_imagesRepositoryFolder)
            .Select(Path.GetFileName)
            .Where(x => x is not null)!
            .ToHashSet<string>();
    }

    private void OnCreated(object source, FileSystemEventArgs e) => UpdateFoldersCache();
    private void OnDeleted(object source, FileSystemEventArgs e) => UpdateFoldersCache();
    private void OnRenamed(object source, RenamedEventArgs e) => UpdateFoldersCache();

    // Dispose the FileSystemWatcher when it's no longer needed.
    public void Dispose()
    {
        _watcher.Dispose();
    }

    public bool HasPicture(string word)
    {
        var lowercaseWord = word.ToLowerInvariant();

        // Directly?
        var hasPictureDirectly = _subfolderListCache.Contains(lowercaseWord);
        if (hasPictureDirectly) return true;

        // Via linked words?
        var linkedWords = _wordsLinker.GetAllLinkedWords(lowercaseWord);
        var hasPictureViaAnyLinkedWord = linkedWords.Any(lw => _subfolderListCache.Contains(lw.ToLowerInvariant()));

        return hasPictureViaAnyLinkedWord;
    }
}
