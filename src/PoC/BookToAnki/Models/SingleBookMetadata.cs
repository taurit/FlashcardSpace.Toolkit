namespace BookToAnki.Models;

public class SingleBookMetadata
{
    public SingleBookMetadata(
        string bookFolder,
        List<string> audioFilesRelativePaths,
        string sentencesEnPath,

        string sentencesEnUkPath,
        string sentencesUkPath,
        string sentencesUkEnPath,

        string sentencesEnPlPath,
        string sentencesPlPath,
        string sentencesPlEnPath,

        string sentencesUkPlPath)
    {
        Parts = audioFilesRelativePaths.Select(x => new SingleBookPart(bookFolder, x)).ToList();
        BookTitle = bookFolder;
        SentencesEnPath = sentencesEnPath;
        SentencesEnUkPath = sentencesEnUkPath;
        SentencesUkPath = sentencesUkPath;
        SentencesUkEnPath = sentencesUkEnPath;
        SentencesEnPlPath = sentencesEnPlPath;
        SentencesPlPath = sentencesPlPath;
        SentencesPlEnPath = sentencesPlEnPath;
        SentencesUkPlPath = sentencesUkPlPath;
    }

    public readonly List<SingleBookPart> Parts;

    public string BookTitle { get; }
    public string SentencesEnPath { get; }
    public string SentencesEnUkPath { get; }
    public string SentencesUkPath { get; }
    public string SentencesUkEnPath { get; }
    public string SentencesEnPlPath { get; }
    public string SentencesPlPath { get; }
    public string SentencesPlEnPath { get; }
    public string SentencesUkPlPath { get; }
}
