using BookToAnki.Models;
using BookToAnki.Services;
using BookToAnki.UI.Components;
using BookToAnki.UI.ViewModels;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace BookToAnki.UI;

public partial class MainWindow
{
    private async void GenerateDalleImages_OnClick(object sender, RoutedEventArgs e)
    {
        var wordsWithoutImages = Enumerable
            .OfType<WordDataViewModel>(WordsDataGrid.Items)
            .Where(x => !x.HasPicture)
            .Where(x => x.Word.Word.ToLowerInvariant().First() == x.Word.Word.First())
            .Where(x => x.Word.Word.Length > 4)
            .Take(15)
            .ToList();

        var wordToExplains = wordsWithoutImages
            .Select(x =>
                new WordToExplain(x.Word.Word, x.Word.UsageExamples.MinBy(z => z.QualityPenalty).Sentence.Text))
            .ToList();

        var dalleImageGenerator =
            new ExplanatoryImageGenerator(_openAiService, _dalleService, Settings.ImagesRepositoryFolder);
        await dalleImageGenerator.BatchGenerateImages(wordToExplains);

        MessageBox.Show($"Finished generating images for {wordsWithoutImages.Count} words!");
    }

    private async void StartWordLinkingFlow_OnClick(object sender, RoutedEventArgs e)
    {
        Stopwatch s = Stopwatch.StartNew();
        // get list of words that are most likely the same ones
        ConcurrentBag<WordSimilarity> similarities = new ConcurrentBag<WordSimilarity>();

        Parallel.For(0, ViewModel.Words.Count, word1Index =>
        {
            for (var word2Index = word1Index + 1; word2Index < ViewModel.Words.Count; word2Index++)
            {
                var word1Vm = ViewModel.Words[word1Index];
                var word2Vm = ViewModel.Words[word2Index];

                var word1 = word1Vm.Word.Word;
                var word2 = word2Vm.Word.Word;

                if (_wordsLinker.AreWordsLinked(word1, word2)) continue;

                var similarityScore = _ukrainianWordSimilarityEvaluator.CalculateSimilarity(word1, word2).Result;
                if (similarityScore < 0.94) continue; // arbitrary choice to avoid veeery large list NxN here

                var word1Example = word1Vm.Word.UsageExamples.First().Sentence.Text;
                var word1ExamplePl = word1Vm.Word.UsageExamples.First().SentenceHumanTranslationPolish ??
                                     word1Vm.Word.UsageExamples.First().SentenceMachineTranslationPolish ??
                                     word1Vm.Word.UsageExamples.First().SentenceHumanTranslationEnglish ??
                                     word1Vm.Word.UsageExamples.First().SentenceMachineTranslationEnglish
                                     ;

                var word2Example = word2Vm.Word.UsageExamples.First().Sentence.Text;
                var word2ExamplePl = word2Vm.Word.UsageExamples.First().SentenceHumanTranslationPolish ??
                                     word2Vm.Word.UsageExamples.First().SentenceMachineTranslationPolish ??
                                     word2Vm.Word.UsageExamples.First().SentenceHumanTranslationEnglish ??
                                     word2Vm.Word.UsageExamples.First().SentenceMachineTranslationEnglish;

                var word1Translation = word1Vm.Word.UsageExamples.First().PolishTranslationOfTheWordNominative;
                var word2Translation = word2Vm.Word.UsageExamples.First().PolishTranslationOfTheWordNominative;


                var similarity = new WordSimilarity(word1, word2, similarityScore, word1Example, word2Example, word1Translation, word2Translation, word1ExamplePl, word2ExamplePl);

                similarities.Add(similarity);
            }
        });

        var similaritiesByScore = similarities.OrderByDescending(x => x.Similarity).ToList();

        s.Stop();
        Debug.WriteLine($"Calculating similarities took {s.Elapsed.TotalSeconds} s");

        var flow = new WordLinkingFlow(similaritiesByScore, _wordsLinker);
        flow.ShowDialog();
    }
}
