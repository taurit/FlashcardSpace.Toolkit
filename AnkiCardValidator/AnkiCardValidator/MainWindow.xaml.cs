using AnkiCardValidator.Models;
using AnkiCardValidator.Utilities;
using AnkiCardValidator.ViewModels;
using System.Windows;

namespace AnkiCardValidator;

public partial class MainWindow : Window
{
    MainWindowViewModel ViewModel => (MainWindowViewModel)this.DataContext;

    public MainWindow()
    {
        InitializeComponent();
        this.DataContext = new MainWindowViewModel();
    }

    private void LoadFlashcards_OnClick(object sender, RoutedEventArgs e)
    {
        var notes = AnkiHelpers.GetAllNotesFromSpecificDeck(Settings.AnkiDatabaseFilePathDev, "1. Spanish", 6);

        ViewModel.Flashcards.Clear();
        foreach (var flashcard in notes)
        {
            var flashcardViewModel = new FlashcardViewModel(flashcard, flashcard.FrontSide, flashcard.BackSide, flashcard.Tags, CefrClassification.Unknown, null, null, null);

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
