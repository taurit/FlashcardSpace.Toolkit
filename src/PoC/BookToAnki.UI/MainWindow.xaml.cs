using BookToAnki.Models;
using BookToAnki.NotePropertiesDatabase;
using BookToAnki.Services;
using BookToAnki.Services.OpenAi;
using BookToAnki.UI.Components;
using BookToAnki.UI.Infrastructure;
using BookToAnki.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using PropertyChanged;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace BookToAnki.UI;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
[AddINotifyPropertyChangedInterface]
public partial class MainWindow : Window
{
    private readonly AnkiNoteGenerator _ankiNoteGenerator;
    private readonly AudioExampleProvider _audioExampleProvider;
    private readonly DalleServiceWrapper _dalleService;
    private readonly List<SingleBookMetadata>? _discoveredBooks;
    private readonly HasPictureService _hasPictureService;

    private readonly ImageCompression _imageCompression;
    private readonly NoteProperties _noteProperties;
    private readonly OpenAiServiceWrapper _openAiService;
    private readonly PartOfSpeechDictionary _partsOfSpeech;
    private readonly IServiceProvider _serviceProvider;
    private readonly UkrainianStressHighlighterWithCache _ukrainianStressHighlighter;
    private readonly UkrainianWordExplainer _ukrainianWordExplainer;
    private readonly UkrainianWordSimilarityEvaluator _ukrainianWordSimilarityEvaluator;
    private readonly WordsLinker _wordsLinker;

    private UkrainianAnkiNote _currentAnkiNoteProposition;

    public MainWindow(IServiceProvider serviceProvider, OpenAiServiceWrapper openAiService,
        LocalWebServer localWebServer, BookListLoader bookListLoader, ImageCompression imageCompression,
        DalleServiceWrapper dalleService, NoteProperties noteProperties, PartOfSpeechDictionary partOfSpeechDictionary,
        UkrainianStressHighlighterWithCache ukrainianStressHighlighter, UkrainianWordExplainer ukrainianWordExplainer,
        AnkiNoteGenerator ankiNoteGenerator, AudioExampleProvider audioExampleProvider,
        UkrainianWordSimilarityEvaluator ukrainianWordSimilarityEvaluator, WordsLinker wordsLinker,
        HasPictureService hasPictureService)
    {
        _serviceProvider = serviceProvider;
        _openAiService = openAiService;
        _imageCompression = imageCompression;
        _dalleService = dalleService;
        _noteProperties = noteProperties;
        _partsOfSpeech = partOfSpeechDictionary;
        _ukrainianStressHighlighter = ukrainianStressHighlighter;
        _ukrainianWordExplainer = ukrainianWordExplainer;
        _ankiNoteGenerator = ankiNoteGenerator;
        _audioExampleProvider = audioExampleProvider;
        _ukrainianWordSimilarityEvaluator = ukrainianWordSimilarityEvaluator;
        _wordsLinker = wordsLinker;
        _hasPictureService = hasPictureService;

        InitializeComponent();
        DataContext = this;
        MainMenu.DataContext = ViewModel;
        WordsDataGrid.DataContext = ViewModel;
        UsageDataGrid.DataContext = ViewModel;
        SimilarWordsDataGrid.DataContext = ViewModel;
        MainWindowStatusBar.DataContext = ViewModel;
        DragEnter += MainWindow_DragEnter;
        Drop += MainWindow_Drop;
        localWebServer.StartLocalServer(Settings.RootServerFolder);
        MultiSelectListBox.ItemsSource = _partsOfSpeech.PartsOfSpeech;

        _discoveredBooks = bookListLoader.GetBookList(Settings.BooksRootFolder).ToList();

        LoadWordsFromBooks(_discoveredBooks);
    }

    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Needed for XAML bindings to work")]
    public MainWindowViewModel ViewModel { get; set; } = new();

    private void RateNotes_OnClick(object sender, RoutedEventArgs e)
    {
        _serviceProvider.GetService<NoteRatingFlow>()!.Display(ViewModel.Words);
    }

    private async void LoadWordsFromBooks(List<SingleBookMetadata> books)
    {
        var wordListsFromAllBooks = new ConcurrentBag<List<WordData>>();
        await Parallel.ForEachAsync(books, async (book, ct) =>
        {
            var bookLoader =
                new BookLoader(_ukrainianWordExplainer); // not thread safe, so each thread gets a new instance
            var wordsFromBook =
                await bookLoader.LoadWordsFromBook(book, Settings.SentenceMatchesCacheFolder);
            var wordFromThisParticularBook = wordsFromBook.wordsData
                .Select(x => new WordData(x.Word, x.NumOccurrences, x.UsageExamples))
                .ToList();
            wordListsFromAllBooks.Add(wordFromThisParticularBook);
        });

        var wordsFromAllBooks = MergeWordsFromMultipleBooks(wordListsFromAllBooks);
        DeduplicateUsageExamples(wordsFromAllBooks);
        CalculateAndUpdateUsageExamplesUsefulness(wordsFromAllBooks);
        SortUsageExamplesByQuality(wordsFromAllBooks);

        var wordsStillConsidered = wordsFromAllBooks
            .Where(x => x.Occurrences > 0)
            .Where(x => x.UsageExamples.Count > 0)
            .Where(x => x.UsageExamples.Any(z => z.TranscriptMatches.Count > 0))
            .ToList();

        RemoveExamplesBelowAcceptableQualityThreshold(wordsStillConsidered);

        var wordsList = new List<WordDataViewModel>();
        foreach (var wordData in wordsStillConsidered)
        {
            if (!wordData.UsageExamples.Any()) continue;

            var partOfSpeech = _partsOfSpeech.WordToPartOfSpeech.ContainsKey(wordData.Word)
                ? _partsOfSpeech.WordToPartOfSpeech[wordData.Word]
                : null;

            if (partOfSpeech == "nouns" /*|| partOfSpeech == "verbs"*/ && wordData.Word.ToLower() == wordData.Word)
            {
                // only nouns because that's what I focus on in my product now
                // skip own names

                var hasPicture = _hasPictureService.HasPicture(wordData.Word);
                var isLinkedToAnyWord = _wordsLinker.IsWordLinkedWithAnyOther(wordData.Word);
                var model = new WordDataViewModel(wordData, hasPicture, false, isLinkedToAnyWord, partOfSpeech);
                wordsList.Add(model);
            }
        }

        //ViewModel.Words = new ObservableCollection<WordDataViewModel>(wordsList.OrderByDescending(x => x.Occurrences));
        ViewModel.Words = new ObservableCollection<WordDataViewModel>(wordsList.OrderByDescending(x =>
            _wordsLinker.GetAllLinkedWords(x.Word.Word).Count() - (x.HasPicture ? 100 : 0)));
        ViewModel.NumberUniqueWordsInAllProcessedBooks = wordsFromAllBooks.Count;

        //await _ukrainianWordSimilarityEvaluator.PrimeCache(ViewModel.Words.Select(x => x.Word.Word).ToList());

        ViewModel.StartupTime = StartupPerformanceMeasurement.Stop();
        SetNotBusy();
    }

    private void RemoveExamplesBelowAcceptableQualityThreshold(List<WordData> wordsFromAllBooks)
    {
        const int highestAcceptableQualityPenalty = 5; // chosen arbitrarily based on whether I liked example or not
        foreach (var word in wordsFromAllBooks)
            word.UsageExamples.RemoveAll(usageExample =>
                usageExample.QualityPenalty > highestAcceptableQualityPenalty
                ||
                usageExample.TranscriptMatches.Count == 0
            );
    }

    private void SortUsageExamplesByQuality(List<WordData> wordsFromAllBooks)
    {
        foreach (var word in wordsFromAllBooks)
            word.UsageExamples.Sort((x, y) => x.QualityPenalty.CompareTo(y.QualityPenalty));
    }

    private void DeduplicateUsageExamples(List<WordData> wordsFromAllBooks)
    {
        foreach (var word in wordsFromAllBooks)
        {
            var encounteredSentences = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
            List<WordUsageExample> toRemove = new();
            foreach (var example in word.UsageExamples)
                if (encounteredSentences.Contains(example.Sentence.Text))
                    toRemove.Add(example);
                else
                    encounteredSentences.Add(example.Sentence.Text);

            foreach (var example in toRemove)
                word.UsageExamples.Remove(example);
        }
    }

    private void CalculateAndUpdateUsageExamplesUsefulness(List<WordData> wordsFromAllBooks)
    {
        var allSentences = wordsFromAllBooks.SelectMany(x => x.UsageExamples).Select(x => x.Sentence).ToList();
        var allUniqueSentences = allSentences.Distinct().ToList();

        if (allSentences.Count == allUniqueSentences.Count)
            // very suspicious. Same examples are expected to be used in all words from an example
            throw new InvalidOperationException(
                "Calculation might be incorrect, some sentences might occur duplicated in data and same word from the book might be calculated many times.");

        var wordRanking = new WordRanking(allUniqueSentences.SelectMany(x => x.Words));

        foreach (var word in wordsFromAllBooks)
        {
            var numUsagesWord = wordRanking.HowManyUsages(word.Word);

            foreach (var wordUsageExample in word.UsageExamples)
                wordUsageExample.NumWordsWithLessUsages = wordUsageExample.Sentence.Words
                    .Where(x => x != word.Word)
                    .Count(otherWord => wordRanking.HowManyUsages(otherWord) < numUsagesWord);
        }
    }

    private List<WordData> MergeWordsFromMultipleBooks(ConcurrentBag<List<WordData>> wordListsFromAllBooks)
    {
        var wordsToWordData = new Dictionary<string, WordData>(
            wordListsFromAllBooks.Count * wordListsFromAllBooks.First().Count,
            StringComparer.InvariantCultureIgnoreCase);

        // order by something stable, to get repeatable outcome - the order of books within wordListsFromAllBook is not deterministic
        foreach (var wordList in wordListsFromAllBooks.OrderByDescending(x => x.Count))
            foreach (var wordData in wordList)
                if (!wordsToWordData.ContainsKey(wordData.Word))
                    wordsToWordData[wordData.Word] = wordData;
                else
                    wordsToWordData[wordData.Word] += wordData;

        return wordsToWordData.Values.OrderByDescending(x => x.Occurrences).ToList();
    }

    private void SimilarWordsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // something was unselected, e.g. when main word changes
        if (e.AddedItems.Count == 0) return;

        SetBusy();
        if (SimilarWordsDataGrid.SelectedItem is WordDataViewModel selectedItem) LoadWordUsagesFor(selectedItem);
        SetNotBusy();
    }

    private async void WordsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        SetBusy();
        if (WordsDataGrid.SelectedItem is WordDataViewModel selectedItem)
        {
            LoadWordUsagesFor(selectedItem);
            await UpdateSimilarWords(selectedItem);
        }

        SetNotBusy();
    }

    private void LoadWordUsagesFor(WordDataViewModel selectedItem)
    {
        var wordsUsages = selectedItem.Word
            .UsageExamples
            .Select(x => new WordUsageExampleViewModel(
                    x,
                    x.Word,
                    x.Sentence,
                    x.TranscriptMatches,
                    x.QualityPenalty,
                    x.SentenceHumanTranslationPolish,
                    x.SentenceHumanTranslationEnglish,
                    _noteProperties.GetNoteRating(new PrefKey(x.Word, x.Sentence.Text))
                )
            )
            .OrderBy(x => x.QualityPenalty)
            .ToList();

        ViewModel.SelectedWordUsages = new ObservableCollection<WordUsageExampleViewModel>(wordsUsages);

        if (wordsUsages.Any())
        {
            var firstWordUsage = wordsUsages.First();
            PictureSelector.SetNewContext(selectedItem.Word.Word, firstWordUsage.SentenceText,
                firstWordUsage.Sentence.PreviousSentence?.Text, firstWordUsage.Sentence.NextSentence?.Text);
        }
    }

    private async Task UpdateSimilarWords(WordDataViewModel selectedItem)
    {
        var linkedWords = _wordsLinker.GetAllLinkedWords(selectedItem.Word.Word).ToList();

        if (!linkedWords.Any())
            linkedWords.Add(selectedItem.Word.Word);

        var mostSimilarWords = await _ukrainianWordSimilarityEvaluator
            .FindMostSimilarWords(linkedWords, ViewModel.Words.Select(x => x.Word.Word).ToList());
        var mostSimilarWordsDictionary = mostSimilarWords.ToDictionary(x => x.Word, x => x);

        var mostSimilarWordsVm = ViewModel.Words
            .Where(x => mostSimilarWordsDictionary.ContainsKey(x.Word.Word) && x != selectedItem).ToList();

        foreach (var vm in mostSimilarWordsVm)
        {
            var similarWord = mostSimilarWordsDictionary[vm.Word.Word];
            vm.SimilarityScore = similarWord.Similarity;

            vm.IsLinkedToSelectedWord = _wordsLinker.AreWordsLinked(similarWord.Word, selectedItem.Word.Word);
            vm.IsLinkedToAnyWord = _wordsLinker.IsWordLinkedWithAnyOther(similarWord.Word);
            vm.HasPicture = _hasPictureService.HasPicture(similarWord.Word);
        }

        var vmsOrderedBySimilarity = mostSimilarWordsVm.OrderByDescending(x => x.SimilarityScore);
        ViewModel.SimilarWords = new ObservableCollection<WordDataViewModel>(vmsOrderedBySimilarity);
    }

    private async void UsageDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var selectedUsage = ViewModel.SelectedWordUsage;
        var selectedWord = ViewModel.SelectedWord;

        if (selectedWord is null) return;
        if (selectedUsage is null) return;

        SetBusy();

        var selectedUsageBusinessModel = selectedUsage.WordUsageExample;

        var imageFolderPath = Path.Combine(Settings.ImagesRepositoryFolder, selectedWord.Word.Word).ToLowerInvariant();
        _currentAnkiNoteProposition =
            await _ankiNoteGenerator.GenerateAnkiNote(selectedUsageBusinessModel, imageFolderPath);
        //_ukrainianStressHighlighter.SaveCache(); // takes time now, and not needed as cache is prepopulated in batch

        ViewModel.TotalCostUsdNumber = _openAiService.TotalCostUsd;

        var ankiCardPreviewWindowContext = new AnkiCardPreviewWindowContext(this, _currentAnkiNoteProposition);
        await CardPreview.SetPreviewWindowHtml(_currentAnkiNoteProposition.UkrainianToPolishCard.PreviewHtml,
            ankiCardPreviewWindowContext, _openAiService, _noteProperties, _audioExampleProvider);

        PictureSelector.SetNewContext(selectedWord.Word.Word, selectedUsage.SentenceText,
            selectedUsage.Sentence.PreviousSentence?.Text, selectedUsage.Sentence.NextSentence?.Text);
        SetNotBusy();
    }


    private void NumWordsInGroup_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_discoveredBooks is not null)
            LoadWordsFromBooks(_discoveredBooks);
    }

    private void MainWindow_DragEnter(object sender, DragEventArgs e)
    {
        // If the DataObject contains file drop list data
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
            // Allow drop
            e.Effects = DragDropEffects.Copy;
        else
            // Do not allow drop
            e.Effects = DragDropEffects.None;
    }

    private void MainWindow_Drop(object sender, DragEventArgs e)
    {
        // If the DataObject contains file drop list data
        var selectedWordRow = WordsDataGrid.SelectedItem as WordDataViewModel;

        if (selectedWordRow is null)
        {
            MessageBox.Show("Could not retrieve selected word group information, adding image failed.");
            return;
        }

        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            var wordFolder = Path.Combine(Settings.ImagesRepositoryFolder, selectedWordRow.Word.Word)
                .ToLowerInvariant();
            if (!Directory.Exists(wordFolder)) Directory.CreateDirectory(wordFolder);

            // Retrieve the file paths
            var filePaths = (string[])e.Data.GetData(DataFormats.FileDrop);

            // Handle each file
            foreach (var filePath in filePaths)
            {
                var newPath = Path.Combine(wordFolder, Path.GetFileName(filePath));
                File.Copy(filePath, newPath, false);
                _imageCompression.ConvertToWebpAndRemoveOriginalImage(newPath);
            }

            //selectedWordRow.HasPicture = true;
            foreach (var wordVm in ViewModel.Words) wordVm.HasPicture = _hasPictureService.HasPicture(wordVm.Word.Word);
        }
    }

    private void WordsDataGrid_OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.C && Keyboard.Modifiers == ModifierKeys.Control)
        {
            var selectedRow = WordsDataGrid.SelectedItem as WordDataViewModel;
            if (selectedRow == null) return;
            var selectedWord = selectedRow.Word.Word;
            Clipboard.SetText(selectedWord);
            e.Handled = true;
        }
    }

    private async void PrimeGptCacheButton_OnClick(object sender, RoutedEventArgs e)
    {
        // let's start with word-level, context-unaware explanations - maybe that would be sufficient
        var allWords = ViewModel.Words
                .Select(x => x.Word)
                .Select(x => new WordToExplain(x.Word, x.UsageExamples.MinBy(z => z.QualityPenalty).Sentence.Text))
                .ToList()
            ;

        const int limit = 1200; // 30 usage examples = ~1 PLN using ChatGPT 4 API
        await _ukrainianWordExplainer.BatchPrepareExplanations(allWords, limit,
            () =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ViewModel.TotalCostUsdNumber = _openAiService.TotalCostUsd;
                });
            });

        ViewModel.TotalCostUsdNumber = _openAiService.TotalCostUsd;
    }

    private async void PrimeStressCachesButton_OnClick(object sender, RoutedEventArgs e)
    {
        var itemsForWhichToPrimeCache = WordsDataGrid.Items.OfType<WordDataViewModel>()
            .Select(x => x.Word)
            .SelectMany(
                x => x.UsageExamples /*.Take(30)*/) // limit priming cache to top quality ones for start, to save resources
            .Where(x => !_ukrainianStressHighlighter.AreStressesCachedAlready(x.Sentence.Text))
            .Select(row => new WordUsageExample(
                row.Word,
                row.Sentence,
                row.TranscriptMatches,
                row.SentenceMachineTranslationPolish,
                row.SentenceMachineTranslationEnglish,
                row.SentenceHumanTranslationPolish,
                row.SentenceHumanTranslationEnglish,
                null))
            .Take(10000)
            .ToList();

        SetBusy();
        await _ankiNoteGenerator.PrimeStressCachesForGeneratingAnkiNotes(itemsForWhichToPrimeCache);
        SetNotBusy();
    }

    private async void PrimeAudioCachesButton_OnClick(object sender, RoutedEventArgs e)
    {
        var itemsForWhichToPrimeCache = WordsDataGrid.Items.OfType<WordDataViewModel>()
            .Select(x => x.Word)
            .SelectMany(x => x.UsageExamples) // limit priming cache to top quality ones for start, to save resources
            .Where(x => x.TranscriptMatches.Any())
            .Select(row => new WordUsageExample(
                row.Word,
                row.Sentence,
                row.TranscriptMatches,
                row.SentenceMachineTranslationPolish,
                row.SentenceMachineTranslationEnglish,
                row.SentenceHumanTranslationPolish,
                row.SentenceHumanTranslationEnglish,
                null))
            .ToList();

        WordsDataGrid.IsEnabled = false;
        try
        {
            await _ankiNoteGenerator.PrimeAudioCachesForGeneratingAnkiNotes(itemsForWhichToPrimeCache);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
        finally
        {
            WordsDataGrid.IsEnabled = true;
        }
    }

    private void MoneyCounter_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        // make sure the counter has the most current value
        ViewModel.TotalCostUsdNumber = _openAiService.TotalCostUsd;
    }

    private async void SimilarWordsDataGrid_OnMouseDoubleClick_ToggleLink(object sender, MouseButtonEventArgs e)
    {
        var selectedWord = ViewModel.SelectedWord;
        var toggledWord = ViewModel.SelectedSimilarWord;

        if (selectedWord is null || toggledWord is null) return;

        if (toggledWord.IsLinkedToNonSelectedWord)
        {
            var group1 = string.Join(", ", _wordsLinker.GetAllLinkedWords(selectedWord.Word.Word));
            var group2 = string.Join(", ", _wordsLinker.GetAllLinkedWords(toggledWord.Word.Word));

            var result =
                MessageBox.Show(
                    $"Are you sure you want to merge word groups of currently unrelated words:\n\n1) {group1}\n2) {group2}?",
                    "Confirm Operation", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
            if (result == MessageBoxResult.No)
            {
                SetNotBusy();
                return;
            }
        }

        SetBusy();

        toggledWord.IsLinkedToSelectedWord = _wordsLinker.ToggleWord(toggledWord.Word.Word, selectedWord.Word.Word);
        toggledWord.IsLinkedToAnyWord = _wordsLinker.IsWordLinkedWithAnyOther(toggledWord.Word.Word);

        foreach (var similarWordVm in ViewModel.SimilarWords)
        {
            similarWordVm.HasPicture = _hasPictureService.HasPicture(similarWordVm.Word.Word);
            similarWordVm.IsLinkedToAnyWord = _wordsLinker.IsWordLinkedWithAnyOther(similarWordVm.Word.Word);
        }

        selectedWord.HasPicture = _hasPictureService.HasPicture(selectedWord.Word.Word);

        await UpdateSimilarWords(selectedWord); // to recalculate similar candidates
        SetNotBusy();
    }

    private void SetBusy()
    {
        if (ViewModel.IsBusy)
            throw new InvalidOperationException(
                "If this exception occurred, some UI element doesn't respect the IsBusy flag and started action when it should have been disabled.");
        ViewModel.IsBusy = true;
    }

    private void SetNotBusy()
    {
        if (!ViewModel.IsBusy)
            throw new InvalidOperationException(
                "If this exception occurred, some code tried to release the lock twice, which is suspicious.");
        ViewModel.IsBusy = false;
    }

    private void SortByPicture_OnClick(object sender, RoutedEventArgs e)
    {
        var newOrder = ViewModel.Words.OrderByDescending(x =>
            _wordsLinker.GetAllLinkedWords(x.Word.Word).Count() - (x.HasPicture ? 100 : 0));
        ViewModel.Words = new ObservableCollection<WordDataViewModel>(newOrder);
    }


    private void PreventSelectionChangeOnAccidentalRightClicks(object sender, MouseButtonEventArgs e)
    {
        // Check if the right mouse button was clicked
        if (e.RightButton == MouseButtonState.Pressed)
        {
            // Find the DataGridRow that was clicked

            if (VisualTreeHelper.GetParent((e.OriginalSource as DependencyObject)!) is DataGridRow row)
                // Prevent the selection change for this row
                e.Handled = true;
        }
    }
}
