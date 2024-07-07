using AnkiCardValidator.Models;
using AnkiCardValidator.Utilities;
using AnkiCardValidator.ViewModels;
using System.Windows;

namespace AnkiCardValidator;

public partial class MainWindow : Window
{
    MainWindowViewModel ViewModel => (MainWindowViewModel)this.DataContext;
    readonly FrequencyDataProvider _spanishFrequencyDataProvider = new(Settings.FrequencyDictionarySpanish);

    public MainWindow()
    {
        InitializeComponent();
        this.DataContext = new MainWindowViewModel();

    }

    private void LoadFlashcards_OnClick(object sender, RoutedEventArgs e)
    {
        _spanishFrequencyDataProvider.LoadFrequencyData();

        var notes = AnkiHelpers.GetAllNotesFromSpecificDeck(Settings.AnkiDatabaseFilePathDev, "1. Spanish", 1000);
        ViewModel.Flashcards.Clear();


        foreach (var flashcard in notes)
        {
            var frequencyPositionFrontSide = _spanishFrequencyDataProvider.GetPosition(flashcard.FrontSide);
            var duplicatesFront = DuplicateDetector.DetectDuplicatesFront(flashcard, notes);
            var duplicatesBack = DuplicateDetector.DetectDuplicatesFront(flashcard, notes);

            var flashcardViewModel = new FlashcardViewModel(flashcard, flashcard.FrontSide, flashcard.BackSide, flashcard.Tags, duplicatesFront, duplicatesBack, frequencyPositionFrontSide, CefrClassification.Unknown, null, null, null);

            ViewModel.Flashcards.Add(flashcardViewModel);
        }
    }

    private async void ValidateCards_OnClick(object sender, RoutedEventArgs e)
    {
        foreach (var flashcard in ViewModel.Flashcards)
        {
            (var evaluationResult, var rawChatGptResponse) = await FlashcardQualityEvaluator.EvaluateFlashcardQuality(flashcard.Note);

            // for easier debugging
            flashcard.RawResponseFromChatGptApi = rawChatGptResponse;

            flashcard.CefrLevel = evaluationResult.CEFRClassification;
            flashcard.QualityIssues = evaluationResult.QualityIssues;
            flashcard.Dialect = evaluationResult.Dialect;

            flashcard.Meanings.Clear();
            foreach (var meaning in evaluationResult.Meanings)
            {
                flashcard.Meanings.Add(meaning);
            }
        }
    }
}
