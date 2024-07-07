using AnkiCardValidator.Models;
using AnkiCardValidator.Utilities;
using AnkiCardValidator.ViewModels;
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

    private void LoadFlashcards_OnClick(object sender, RoutedEventArgs e)
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
    }

    private async void ValidateCards_OnClick(object sender, RoutedEventArgs e)
    {
        var vmBatch = ViewModel.Flashcards.Take(10).ToList();
        var modelBatch = vmBatch.Select(x => x.Note).ToList();
        (var evaluationResult, var rawChatGptResponse) = await FlashcardQualityEvaluator.EvaluateFlashcardsQuality(modelBatch);

        int i = -1;
        foreach (var evaluation in evaluationResult)
        {
            i++;

            var flashcard = vmBatch[i];
            flashcard.CefrLevel = evaluation.CEFRClassification;
            flashcard.QualityIssues = evaluation.Issues;

            flashcard.Meanings.Clear();
            foreach (var meaning in evaluation.Meanings)
            {
                flashcard.Meanings.Add(meaning);
            }
        }

        return;
        //foreach (var flashcard in ViewModel.Flashcards.Take(10))
        //{
        //    (var evaluationResult, var rawChatGptResponse) = await FlashcardQualityEvaluator.EvaluateFlashcardQuality(flashcard.Note);

        //    // for easier debugging
        //    flashcard.RawResponseFromChatGptApi = rawChatGptResponse;

        //    flashcard.CefrLevel = evaluationResult.CEFRClassification;
        //    flashcard.QualityIssues = evaluationResult.QualityIssues;

        //    flashcard.Meanings.Clear();
        //    foreach (var meaning in evaluationResult.Meanings)
        //    {
        //        flashcard.Meanings.Add(meaning);
        //    }
        //}
    }
}
