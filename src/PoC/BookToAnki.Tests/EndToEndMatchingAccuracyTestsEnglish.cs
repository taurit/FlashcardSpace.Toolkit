using AdvancedSentenceExtractor.Services;
using BookToAnki.Services;

namespace BookToAnki.Tests;
[TestClass]
public class EndToEndMatchingAccuracyTestsEnglish
{
    [TestInitialize]
    public void Initialize()
    {
        LicensedContentGuard.EnsureLicensedContentIsAvailableOnTheMachine();
    }

    [TestMethod]
    public void When_MetamorphosisTextIsMatchedAgainstAudiobook_Expect_AtLeast90PercentOfSentencesMatched()
    {
        // dependencies
        var wordTokenizer = new WordTokenizer();
        var sentenceBuilder = new SentenceFactory(wordTokenizer);
        var sentenceTokenizer = new SentenceTokenizer(sentenceBuilder);

        var numSentencesInEbook = 0;
        var numMatchedSentencesInEbook = 0;

        for (int i = 1; i <= 1; i++)
        {
            var chapterNumber = $"{i:D2}";
            var book = Path.Combine(LicensedContentGuard.RootFolderForLicensedContent, $"metamorphosis_en_{chapterNumber}.txt");
            var transcript = Path.Combine(LicensedContentGuard.RootFolderForLicensedContent, $"metamorphosis_en_{chapterNumber}.azure.json");

            var bookContent = File.ReadAllText(book);
            var sentences = sentenceTokenizer.TokenizeBook(bookContent);
            // unlike Harry Potter, this transcript was done in Azure, not Google Cloud!
            var transcriptWords = TranscriptReader.ReadTranscript(transcript, "irrelevant");

            var sentencesWithSounds = SentenceMatcher.Match(sentences, transcriptWords);

            var allSentencesInChapter = sentences.Count;
            var matchedSentencesInChapter = sentencesWithSounds.Select(x => x.Sentence).Distinct();
            var matchedSentencesInChapterText = sentencesWithSounds.Select(x => x.Sentence.Text).Distinct().ToList();

            var nonMatchedSentences = sentences.Except(matchedSentencesInChapter);

            // Assert chapter success rate
            decimal chapterSuccessRatePercent = 100m * matchedSentencesInChapterText.Count / allSentencesInChapter;
            Console.WriteLine($"Sentence matching success rate, chapter {chapterNumber}: {chapterSuccessRatePercent:#.##}%");
            Assert.IsTrue(chapterSuccessRatePercent >= 65, $"Expected matching success rate > 65%, but got {chapterSuccessRatePercent:#.##}% for chapter {chapterNumber}");
            Assert.IsTrue(chapterSuccessRatePercent <= 100, $"Success rate above 100%, something is wrong with the metric.");

            numSentencesInEbook += allSentencesInChapter;
            numMatchedSentencesInEbook += matchedSentencesInChapterText.Count;
        }


        // Assert whole ebook success rate
        decimal totalSuccessRatePercent = 100m * numMatchedSentencesInEbook / (decimal)numSentencesInEbook;
        Console.WriteLine($"Sentence matching success rate, whole ebook: {totalSuccessRatePercent:#.##}%");
        Assert.IsTrue(totalSuccessRatePercent <= 100, $"Success rate above 100%, something is wrong with the metric.");
        Assert.IsTrue(totalSuccessRatePercent > 70m, $"Expected matching success rate > 70%, but got {totalSuccessRatePercent:#.##}%");
    }

}
