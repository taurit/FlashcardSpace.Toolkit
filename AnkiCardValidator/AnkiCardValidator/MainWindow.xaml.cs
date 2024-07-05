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
            var flashcardViewModel = new FlashcardViewModel(flashcard, flashcard.FrontSide, flashcard.BackSide, CefrClassification.Unknown, new List<Warning>(), "");

            ViewModel.Flashcards.Add(flashcardViewModel);
        }

    }

    private async void EvaluateCards_OnClick(object sender, RoutedEventArgs e)
    {
        var evaluationResult = await FlashcardQualityEvaluator.EvaluateFlashcardQuality(ViewModel.SelectedFlashcard.Note);
        ViewModel.SelectedFlashcard.CefrClassification = evaluationResult.FrontSideCEFRClassification;
        ViewModel.SelectedFlashcard.Comments = String.IsNullOrWhiteSpace(evaluationResult.Comments) ? "None" : evaluationResult.Comments;

        Debug.WriteLine(evaluationResult);
    }
}
