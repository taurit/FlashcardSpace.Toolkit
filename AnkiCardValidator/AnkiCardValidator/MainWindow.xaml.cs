using AnkiCardValidator.Models;
using AnkiCardValidator.Utilities;
using AnkiCardValidator.ViewModels;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Windows;

namespace AnkiCardValidator;

public partial class MainWindow : Window
{
    MainWindowViewModel ViewModel => (MainWindowViewModel)this.DataContext;
    readonly FrequencyDataProvider _spanishFrequencyDataProvider = new(Settings.FrequencyDictionarySpanish);
    readonly FrequencyDataProvider _polishFrequencyDataProvider = new(Settings.FrequencyDictionaryPolish);

    public MainWindow()
    {
        InitializeComponent();
        this.DataContext = new MainWindowViewModel();
    }

    private async void LoadFlashcards_OnClick(object sender, RoutedEventArgs e)
    {
        _spanishFrequencyDataProvider.LoadFrequencyData();
        _polishFrequencyDataProvider.LoadFrequencyData();

        var notes = AnkiHelpers.GetAllNotesFromSpecificDeck(Settings.AnkiDatabaseFilePathDev, "1. Spanish", null);
        ViewModel.Flashcards.Clear();

        foreach (var flashcard in notes)
        {
            var frequencyPositionFrontSide = _spanishFrequencyDataProvider.GetPosition(flashcard.FrontSide);
            var frequencyPositionBackSide = _polishFrequencyDataProvider.GetPosition(flashcard.BackSide);

            var duplicatesFront = DuplicateDetector.DetectDuplicatesFront(flashcard, notes);
            var duplicatesBack = DuplicateDetector.DetectDuplicatesFront(flashcard, notes);

            var flashcardViewModel = new FlashcardViewModel(flashcard, flashcard.FrontSide, flashcard.BackSide, flashcard.Tags, duplicatesFront, duplicatesBack, frequencyPositionFrontSide, frequencyPositionBackSide, CefrClassification.Unknown, null, null);

            ViewModel.Flashcards.Add(flashcardViewModel);
        }

        await ReloadFlashcardsEvaluationAndSortByMostPromising();
    }

    private async void EvaluateFewMoreCards_OnClick(object sender, RoutedEventArgs e)
    {
        var batchSize = Int32.Parse(BatchSize.Text);

        // Load as much as you can from the local cache
        var vmsForWhichCacheExists = ViewModel.Flashcards.Where(x => File.Exists(GenerateCacheFilePath(x))).ToList();

        // Sort the rest by the most promising ones (lowest penalty score)
        var notesWithEvaluationMissing = ViewModel.Flashcards.Except(vmsForWhichCacheExists).OrderBy(x => x.Penalty).ToList();

        var batchOfVms = notesWithEvaluationMissing.Take(batchSize).ToList();
        var batchOfNotes = batchOfVms.Select(x => x.Note).ToList();
        var evaluationResult = await FlashcardQualityEvaluator.EvaluateFlashcardsQuality(batchOfNotes);

        int i = 0;
        var jsonSerializerOptions = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true
        };
        foreach (var vm in batchOfVms)
        {
            var evaluation = evaluationResult.Evaluations[i];

            // save to cache
            var cacheFilePath = GenerateCacheFilePath(vm);
            var cacheContent = new FlashcardQualityEvaluationCacheModel(evaluation, evaluationResult.RawChatGptResponse);
            var cacheContentSerialized = JsonSerializer.Serialize(cacheContent, jsonSerializerOptions);
            await File.WriteAllTextAsync(cacheFilePath, cacheContentSerialized);

            i++;
        }

        await ReloadFlashcardsEvaluationAndSortByMostPromising();
    }

    private async Task ReloadFlashcardsEvaluationAndSortByMostPromising()
    {
        foreach (var vm in ViewModel.Flashcards)
        {
            await TryUpdateViewModelWithEvaluationData(vm);
        }

        ViewModel.Flashcards = new ObservableCollection<FlashcardViewModel>(ViewModel.Flashcards.OrderBy(x => x.Penalty));
    }

    private static string GenerateCacheFilePath(FlashcardViewModel flashcardVm)
    {
        var cacheFileName = $"eval-{Settings.OpenAiModelId}_{flashcardVm.Note.Id}.txt";
        var cacheFilePath = Path.Combine(Settings.GptResponseCacheDirectory, cacheFileName);
        return cacheFilePath;
    }

    private static async Task TryUpdateViewModelWithEvaluationData(FlashcardViewModel flashcardVm)
    {
        var cacheFilePath = GenerateCacheFilePath(flashcardVm);
        if (!File.Exists(cacheFilePath)) return;

        var cacheContent = await File.ReadAllTextAsync(cacheFilePath);

        var cached = JsonSerializer.Deserialize<FlashcardQualityEvaluationCacheModel>(cacheContent);

        flashcardVm.CefrLevel = cached.Evaluation.CEFR;
        flashcardVm.QualityIssues = cached.Evaluation.Issues;
        flashcardVm.RawResponseFromChatGptApi = cached.RawChatGptResponse;

        flashcardVm.Meanings.Clear();
        foreach (var meaning in cached.Evaluation.Meanings)
        {
            flashcardVm.Meanings.Add(meaning);
        }
    }
}

internal record FlashcardQualityEvaluationCacheModel(FlashcardQualityEvaluation Evaluation, string RawChatGptResponse);
