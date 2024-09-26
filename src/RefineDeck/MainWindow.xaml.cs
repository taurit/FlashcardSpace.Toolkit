using CoreLibrary.Models;
using CoreLibrary.Services.AnkiExportService;
using RefineDeck.Utils;
using RefineDeck.ViewModels;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RefineDeck;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    MainWindowViewModel ViewModel => (MainWindowViewModel)DataContext;


    public MainWindow()
    {
        InitializeComponent();

        DataContext = new MainWindowViewModel();
        ViewModel.Deck = DeckLoader.LoadDeck();

        ViewModel.PropertyChanged += ViewModel_PropertyChanged;

        // load preview component in WebView
        UpdateFlashcardPreview();
    }

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        //MessageBox.Show($"{e.PropertyName} changed; updating sub to SelectedFlashcard");
        if (ViewModel.SelectedFlashcard != null)
        {
            // unsubscribe from eventual previous subs to avoid multiple executions
            ViewModel.SelectedFlashcard.PropertyChanged -= PropertyOfSelectedFlashcardChanged;
            ViewModel.SelectedFlashcard.PropertyChanged += PropertyOfSelectedFlashcardChanged;
        }
    }

    private void PropertyOfSelectedFlashcardChanged(object? sender, PropertyChangedEventArgs e)
    {
        UpdateFlashcardPreview();
    }

    private async void FlashcardSelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
    {
        UpdateFlashcardPreview();
    }

    private async void UpdateFlashcardPreview()
    {

        // update WebView preview
        await WebViewHelper.EnsureWebViewIsInitialized(this.Preview);

        // todo: un-hardcode path to previewer
        var recentBuildOfPreviewer = "d:\\Projekty\\FlashcardSpace.Toolkit\\src\\DeckBrowser\\dist\\index.html";
        var targetFilePath = ViewModel.Deck.DeckPath.PreviewIndexHtmlPath;
        File.Copy(recentBuildOfPreviewer, targetFilePath, true);
        Preview.Source = new Uri(targetFilePath);

        var selectedFlashcard = ViewModel.SelectedFlashcard;
        if (selectedFlashcard is null) return;

        // Call a JavaScript function with arguments
        var flashcards = new List<FlashcardNote> {
                new FlashcardNote {
                    Term = selectedFlashcard.Term,
                    TermAudio = selectedFlashcard.TermAudio,
                    TermStandardizedForm = selectedFlashcard.OriginalFlashcard.TermStandardizedForm,

                    TermTranslation = selectedFlashcard.TermTranslation,
                    TermTranslationAudio = selectedFlashcard.TermTranslationAudio,
                    TermStandardizedFormEnglishTranslation = selectedFlashcard.OriginalFlashcard.TermStandardizedFormEnglishTranslation,

                    Remarks = selectedFlashcard.OriginalFlashcard.Remarks,

                    Context = selectedFlashcard.SentenceExample,
                    ContextAudio = selectedFlashcard.SentenceExampleAudio,
                    ContextTranslation = selectedFlashcard.SentenceExampleTranslation,
                    ContextTranslationAudio = selectedFlashcard.OriginalFlashcard.ContextTranslationAudio,
                    ContextEnglishTranslation = selectedFlashcard.OriginalFlashcard.ContextEnglishTranslation,

                    Type = selectedFlashcard.OriginalFlashcard.Type,
                    ImageCandidates = selectedFlashcard.OriginalFlashcard.ImageCandidates,
                    SelectedImageIndex = selectedFlashcard.SelectedImageIndex >= selectedFlashcard.OriginalFlashcard.ImageCandidates.Count ? null : selectedFlashcard.SelectedImageIndex
                }
        };

        var deck = new Deck("Preview deck", flashcards, ViewModel.Deck.MediaFileNamePrefix, ViewModel.Deck.SourceLanguage, ViewModel.Deck.TargetLanguage);
        var cardPreviewSerialized = deck.Serialize();
        await Preview.CoreWebView2.ExecuteScriptAsync($"window.setDataFromWpf({cardPreviewSerialized});");
    }


    private void ImagesPanel_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        // Horizontal scrolling using mouse wheel
        var wrapPanel = (sender as ListBox);
        var scrollViewer = VisualTreeHelpers.FindVisualChild<ScrollViewer>(wrapPanel);
        if (scrollViewer.ComputedHorizontalScrollBarVisibility == Visibility.Visible)
        {
            scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - e.Delta);
            e.Handled = true; // Prevent vertical scrolling
        }
    }

    private void ResetValue_OnClick(object sender, RoutedEventArgs e)
    {
        var f = ViewModel.SelectedFlashcard;
        if (f is null) return;

        var senderButton = (Button)sender;
        var resetValue = senderButton.Tag;

        switch (resetValue)
        {
            case "Term":
                f.Term = f.OriginalFlashcard.Term;
                f.TermAudio = f.OriginalFlashcard.TermAudio;
                break;
            case "TermTranslation":
                f.TermTranslation = f.OriginalFlashcard.TermTranslation;
                f.TermTranslationAudio = f.OriginalFlashcard.TermTranslationAudio;
                break;
            case "SentenceExample":
                f.SentenceExample = f.OriginalFlashcard.Context;
                f.SentenceExampleAudio = f.OriginalFlashcard.ContextAudio;
                break;
            case "SentenceExampleTranslation":
                f.SentenceExampleTranslation = f.OriginalFlashcard.ContextTranslation;
                break;
            case "Remarks":
                f.Remarks = f.OriginalFlashcard.Remarks;
                break;
            default:
                throw new NotImplementedException($"Not implemented: resetting the {resetValue} field.");
        }

        SaveChanges();
    }


    private void ShowDiffInVsCode_OnClick(object sender, RoutedEventArgs e)
    {
        var file1 = ViewModel.Deck.DeckPath.DeckManifestPath;
        var file2 = ViewModel.Deck.DeckPath.DeckManifestEditsPath;
        //code --diff <file1> <file2>

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "C:\\Users\\windo\\AppData\\Local\\Programs\\Microsoft VS Code\\Code.exe",
                Arguments = $"--diff {file1} {file2}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            }
        };
        process.Start();
    }

    private void Approve_OnClick(object sender, RoutedEventArgs e)
    {
        ChangeFlashcardStatusAndAdvance(ApprovalStatus.Approved);
    }


    private void Reject_OnClick(object sender, RoutedEventArgs e)
    {
        ChangeFlashcardStatusAndAdvance(ApprovalStatus.Rejected);
    }

    private void ChangeFlashcardStatusAndAdvance(ApprovalStatus newStatus)
    {
        var currentFlashcard = ViewModel.SelectedFlashcard;
        if (currentFlashcard is null) return;

        currentFlashcard.ApprovalStatus = newStatus;

        SelectNextFlashcard(currentFlashcard);
    }

    private void SelectNextFlashcard(ReviewedCardViewModel f)
    {
        // select next flashcard
        var nextIndex = ViewModel.Deck.Flashcards.IndexOf(f) + 1;
        if (nextIndex < ViewModel.Deck.Flashcards.Count)
        {
            ViewModel.SelectedFlashcard = ViewModel.Deck.Flashcards[nextIndex];
        }

        this.SaveChanges();
    }

    private void SaveChanges_OnClick(object sender, RoutedEventArgs e) => SaveChanges();

    private void SaveChanges()
    {
        DeckLoader.SaveChangesInDeck(ViewModel.Deck);
    }


    private void ExportToAnkiDeck_OnClick(object sender, RoutedEventArgs e)
    {
        SaveChanges();

        var ankiExportService = new AnkiExportService();
        ankiExportService.ExportToAnki(ViewModel.Deck.DeckPath);

        // open folder where the deck was created in Explorer
        Process.Start("explorer.exe", ViewModel.Deck.DeckPath.AnkiExportPath);
    }

    // Updates audio files 
    private async void UpdateAudio_OnClick(object sender, RoutedEventArgs e)
    {
        var audioProvider = AudioPatcher.GetAudioProviderInstance(ViewModel.Deck.DeckPath);

        var card = ViewModel.SelectedFlashcard;
        var tag = ((Button)sender).Tag.ToString();

        switch (tag)
        {
            case "Term":
                var newAudioFilePathTerm = await audioProvider.GenerateAudioOrUseCached(card.Term, ViewModel.Deck.SourceLanguage);
                card.TermAudio = AudioPatcher.ToRelativePath(newAudioFilePathTerm, ViewModel.Deck.DeckPath);
                break;
            case "TermTranslation":
                var newAudioFilePathTermTranslation = await audioProvider.GenerateAudioOrUseCached(card.TermTranslation, ViewModel.Deck.TargetLanguage);
                card.TermTranslationAudio = AudioPatcher.ToRelativePath(newAudioFilePathTermTranslation, ViewModel.Deck.DeckPath);
                break;
            case "SentenceExample":
                var newAudioFilePathSentenceExample = await audioProvider.GenerateAudioOrUseCached(card.SentenceExample, ViewModel.Deck.SourceLanguage);
                card.SentenceExampleAudio = AudioPatcher.ToRelativePath(newAudioFilePathSentenceExample, ViewModel.Deck.DeckPath);
                break;

            default:
                throw new NotImplementedException($"Not implemented: updating the {tag} audio.");
        }


    }

}
