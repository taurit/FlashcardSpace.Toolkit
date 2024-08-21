namespace BookToAnki.Tests;

/// <summary>
/// Some tests perform matching of full audiobooks transcripts against the full text of the book.
/// 
/// Since ebook content is often not in public domain, I cannot include the text or the audiobook transcript in the git repository.
/// Tests will only run on my (main developer's) machine, giving `inconclusive` result everywhere else.
///
/// This PoC is unlikely to be developed further, but if it was ever needed, we could replace proprietary content with some
/// public domain books to resolve this issue.
/// </summary>
internal static class LicensedContentGuard
{
    internal const string RootFolderForLicensedContent = "d:\\Flashcards\\Words\\BookToAnkiUnitTestResources\\";

    public static void EnsureLicensedContentIsAvailableOnTheMachine()
    {
        if (!Directory.Exists(RootFolderForLicensedContent))
        {
            Assert.Inconclusive("Input files (full ebook content) were not found on this machine. " +
                                "For legal reasons, ebooks are not distributed as part of git repository, so it is expected on " +
                                "most machines, unless ebooks were distributed via some other channel.");
        }
    }

}
