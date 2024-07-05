using AnkiCardValidator.Models;
using AnkiCardValidator.Utilities;
using AnkiCardValidator.ViewModels;
using System.Diagnostics;
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
        var notes = AnkiHelpers.GetAllNotesFromSpecificDeck(Settings.AnkiDatabaseFilePathDev, "1. Spanish", 10);

        ViewModel.Flashcards.Clear();
        foreach (var flashcard in notes)
        {
            var flashcardViewModel = new FlashcardViewModel(flashcard, flashcard.FrontSide, flashcard.BackSide, CefrClassification.Unknown, "", "",
                [], null, "");

            ViewModel.Flashcards.Add(flashcardViewModel);
        }

    }

    private async void EvaluateCards_OnClick(object sender, RoutedEventArgs e)
    {
        var evaluationResult = await FlashcardQualityEvaluator.EvaluateFlashcardQuality(ViewModel.SelectedFlashcard.Note);
        ViewModel.SelectedFlashcard.CefrClassification = evaluationResult.CEFRClassification;
        ViewModel.SelectedFlashcard.QualityIssues = String.IsNullOrWhiteSpace(evaluationResult.QualityIssues) ? "None" : evaluationResult.QualityIssues;
        ViewModel.SelectedFlashcard.Dialect = String.IsNullOrWhiteSpace(evaluationResult.Dialect) ? "None" : evaluationResult.Dialect;
        ViewModel.SelectedFlashcard.IsFlashcardWorthIncludingForA2LevelStudents = evaluationResult.IsFlashcardWorthIncludingForA2LevelStudents;
        ViewModel.SelectedFlashcard.IsFlashcardWorthIncludingJustification = String.IsNullOrWhiteSpace(evaluationResult.IsFlashcardWorthIncludingJustification) ? "None" : evaluationResult.IsFlashcardWorthIncludingJustification;
        ViewModel.SelectedFlashcard.Meanings.Clear();
        foreach (var meaning in evaluationResult.Meanings)
        {
            ViewModel.SelectedFlashcard.Meanings.Add(meaning);
        }

        Debug.WriteLine(evaluationResult);
    }
}
