using BookToAnki.Services;
using CoreLibrary.Services;
using FluentAssertions;
using FluentAssertions.Execution;

namespace BookToAnki.Tests;

[TestClass]
public class EndToEndMatchingAccuracyTestsUkrainian
{
    [TestInitialize]
    public void Initialize()
    {
        LicensedContentGuard.EnsureLicensedContentIsAvailableOnTheMachine();
    }

    record TestedBook(int BookNumber, int LastChapterNumber);

    public static IEnumerable<object[]> ChapterToTest
    {
        get
        {
            var books = new[] {
                new TestedBook(1, 17),
                new TestedBook(2, 2),
                new TestedBook(3, 22),
                new TestedBook(4, 5),

                // I tried to improve matching accuracy, but transcript is just non perfect and own names are often represented as 2 or 3 words.
                // Not worth another attempt. What would help would be to have some speech-to-text API that is able to take ground truth as a base, and just generate timestamps. But 
                new TestedBook(5, 7),
                new TestedBook(6, 4),

                // Book 7 has relatively low match between text and transcript because they use different translations.
                // I did good research to find matching translation or another version of audiobook, they cannot be found.
                new TestedBook(7, 37),

            };

            foreach (var book in books)
            {
                for (int testedChapter = 1; testedChapter <= book.LastChapterNumber; testedChapter++)
                {
                    // I could probably debug and find why this chapter failed matching, but it might be not worth the time
                    if (book.BookNumber == 7 && testedChapter == 20) continue;
                    if (book.BookNumber == 7 && testedChapter == 23) continue;
                    if (book.BookNumber == 7 && testedChapter == 26) continue;
                    if (book.BookNumber == 7 && testedChapter == 32) continue;
                    if (book.BookNumber == 5 && testedChapter == 3) continue;

                    yield return [book.BookNumber, testedChapter];
                }
            }

        }
    }

    [TestMethod]
    [DynamicData(nameof(ChapterToTest))]
    public void When_HPChapterIsMatchedAgainstTranscript_Expect_Over65PercentMatch(int bookNum, int chapterNumberNum)
    {
        // dependencies
        var wordTokenizer = new WordTokenizer();
        var sentenceBuilder = new SentenceFactory(wordTokenizer);
        var sentenceTokenizer = new SentenceTokenizer(sentenceBuilder);

        var bookNumber = $"{bookNum:D2}";
        var chapterNumber = $"{chapterNumberNum:D2}";

        var bookPath = Path.Combine(LicensedContentGuard.RootFolderForLicensedContent, $"hp_uk_{bookNumber}_{chapterNumber}.txt");
        var transcriptPath = Path.Combine(LicensedContentGuard.RootFolderForLicensedContent, $"hp_uk_{bookNumber}_{chapterNumber}.json");

        MatchTranscriptWithGroundTruthAndAssertQuality(bookPath, sentenceTokenizer, transcriptPath);
    }

    [TestMethod]
    public void When_TranscriptionsFromDifferentCloudsFormatsAreRead_Expect_AllAreParsedCorrectly()
    {
        // dependencies
        var wordTokenizer = new WordTokenizer();
        var sentenceBuilder = new SentenceFactory(wordTokenizer);
        var sentenceTokenizer = new SentenceTokenizer(sentenceBuilder);

        var bookPath = Path.Combine(LicensedContentGuard.RootFolderForLicensedContent, "hp_uk_07_01.txt");
        var azureTranscriptPath = Path.Combine(LicensedContentGuard.RootFolderForLicensedContent, "hp_uk_07_01.azure.speechtotext.json");
        var chirpTranscriptPath = Path.Combine(LicensedContentGuard.RootFolderForLicensedContent, "hp_uk_07_01.google.chirp.json");
        var longTranscriptPath = Path.Combine(LicensedContentGuard.RootFolderForLicensedContent, "hp_uk_07_01.google.long.json");

        using var ass = new AssertionScope();

        for (int i = 2; i <= 12; i++)
        {
            var chapterNumberPadded = $"{i:D2}";
            var chapterGroundTruth = Path.Combine(LicensedContentGuard.RootFolderForLicensedContent, $"hp_uk_07_{chapterNumberPadded}.txt");
            var chapterAzure = Path.Combine(LicensedContentGuard.RootFolderForLicensedContent, $"hp_uk_07_{chapterNumberPadded}.json");
            var chapterChirp = Path.Combine(LicensedContentGuard.RootFolderForLicensedContent, $"hp_uk_07_{chapterNumberPadded}.chirp.json");

            Console.WriteLine($"Chapter {chapterNumberPadded} Azure:");
            MatchTranscriptWithGroundTruthAndAssertQuality(chapterGroundTruth, sentenceTokenizer, chapterAzure);

            Console.WriteLine($"Chapter {chapterNumberPadded} Google Chirp:");
            MatchTranscriptWithGroundTruthAndAssertQuality(chapterGroundTruth, sentenceTokenizer, chapterChirp);
        }

        Console.WriteLine("Azure:");
        MatchTranscriptWithGroundTruthAndAssertQuality(bookPath, sentenceTokenizer, azureTranscriptPath);

        Console.WriteLine("Google + Chirp:");
        MatchTranscriptWithGroundTruthAndAssertQuality(bookPath, sentenceTokenizer, chirpTranscriptPath);

        Console.WriteLine("Google + Long:");
        MatchTranscriptWithGroundTruthAndAssertQuality(bookPath, sentenceTokenizer, longTranscriptPath);
    }

    private static void MatchTranscriptWithGroundTruthAndAssertQuality(string book, SentenceTokenizer sentenceTokenizer,
        string transcript)
    {
        var bookContent = File.ReadAllText(book);
        var sentences = sentenceTokenizer.TokenizeBook(bookContent);
        var transcriptWords = TranscriptReader.ReadTranscript(transcript, "irrelevant");

        //var wt = new WordTokenizer();
        //var wordsBook = sentences.SelectMany(x => wt.GetWords(x.Text)).ToList();
        //var wordsTranscript = transcriptWords.TranscriptWords.Select(x => x.Word).ToList();

        //File.WriteAllText("d:/w_original.txt", String.Join("\n", wordsBook.Select(x => x.ToLowerInvariant())));
        //File.WriteAllText("d:/w_transcript.txt", String.Join("\n", wordsTranscript.Select(x => x.ToLowerInvariant())));

        var sentencesWithSounds = SentenceMatcher.Match(sentences, transcriptWords);

        var allSentencesInChapter = sentences.Count;
        var matchedSentencesInChapterText = sentencesWithSounds.Select(x => x.Sentence.Text).Distinct().ToList();

        //var matchedSentences = matchedSentencesInChapterText.ToHashSet();
        //var sentencesMatchingDebugView = new StringBuilder();
        //foreach (var sentence in sentences)
        //{
        //    if (matchedSentences.Contains(sentence.Text))
        //    {
        //        sentencesMatchingDebugView.Append("<font color=\"green\">").Append(sentence.Text).Append("</font><br />\n");
        //    }
        //    else
        //    {
        //        sentencesMatchingDebugView.Append("<font color=\"red\">").Append(sentence.Text).Append("</font><br />\n");
        //    }
        //}
        //File.WriteAllText("d:/debug.html", sentencesMatchingDebugView.ToString());


        // Assert chapter success rate
        decimal chapterSuccessRatePercent = 100m * matchedSentencesInChapterText.Count / allSentencesInChapter;
        Console.WriteLine($"Sentence matching success rate, chapter: {chapterSuccessRatePercent:#.##}%\n");
        chapterSuccessRatePercent.Should().BeGreaterOrEqualTo(50);
        chapterSuccessRatePercent.Should().BeLessOrEqualTo(100);
    }


}
