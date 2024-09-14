namespace CoreLibrary.Utilities;
public static class PathHelpers
{
    private static readonly HashSet<string> FoldersKnownToExist = new();

    /// <summary>
    /// Optimized way to ensure that the folder exists.
    /// It only calls filesystem API once, and then caches value.
    /// </summary>
    public static void EnsureDirectoryExists(this string path)
    {
        if (FoldersKnownToExist.Contains(path))
            return;

        Directory.CreateDirectory(path);
        FoldersKnownToExist.Add(path);
    }
}
