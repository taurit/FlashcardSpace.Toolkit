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
        var notes = AnkiHelpers.GetAllNotesFromSpecificDeck(Settings.AnkiDatabaseFilePathDev, "1. Spanish", 30);

        ViewModel.Flashcards.Clear();
        foreach (var flashcard in notes)
        {
            var flashcardViewModel = new FlashcardViewModel(flashcard, flashcard.FrontSide, flashcard.BackSide, CefrClassification.Unknown, "", "",
                [], null, "", "");

            ViewModel.Flashcards.Add(flashcardViewModel);
        }

    }

    private async void EvaluateCards_OnClick(object sender, RoutedEventArgs e)
    {
        foreach (var flashcard in ViewModel.Flashcards)
        {
            (var evaluationResult, var rawChatGptResponse) = await FlashcardQualityEvaluator.EvaluateFlashcardQuality(flashcard.Note);

            // for easier debugging
            flashcard.RawResponseFromChatGptApi = rawChatGptResponse;

            flashcard.CefrClassification = evaluationResult.CEFRClassification;
            flashcard.QualityIssues = String.IsNullOrWhiteSpace(evaluationResult.QualityIssues) ? "\u2705" : evaluationResult.QualityIssues;
            flashcard.Dialect = String.IsNullOrWhiteSpace(evaluationResult.Dialect) ? "\u2705" : evaluationResult.Dialect;
            flashcard.IsFlashcardWorthIncludingForA2LevelStudents = evaluationResult.IsFlashcardWorthIncludingForA2LevelStudents;
            flashcard.IsFlashcardWorthIncludingJustification = String.IsNullOrWhiteSpace(evaluationResult.IsFlashcardWorthIncludingJustification) ? "\u2705" : evaluationResult.IsFlashcardWorthIncludingJustification;

            flashcard.Meanings.Clear();
            foreach (var meaning in evaluationResult.Meanings)
            {
                flashcard.Meanings.Add(meaning);
            }
        }
    }
}
