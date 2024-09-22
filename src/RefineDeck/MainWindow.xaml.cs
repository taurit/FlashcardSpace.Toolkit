using CoreLibrary.Models;
using RefineDeck.Utils;
using RefineDeck.ViewModels;
using System.ComponentModel;
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
        var selectedFlashcard = ViewModel.SelectedFlashcard;
        if (selectedFlashcard is null) return;

        // update WebView preview
        await WebViewHelper.EnsureWebViewIsInitialized(this.Preview);

        // todo: un-hardcode path to previewer
        var recentBuildOfPreviewer = "d:\\Projekty\\FlashcardSpace.Toolkit\\src\\DeckBrowser\\dist\\index.html";
        var targetFilePath = Path.Combine(ViewModel.Deck.DeckFolderPath, "..\\index.html");
        File.Copy(recentBuildOfPreviewer, targetFilePath, true);
        Preview.Source = new Uri(targetFilePath);


        // Call a JavaScript function with arguments
        var flashcards = new List<FlashcardNote> {
                new FlashcardNote {
                    Term = selectedFlashcard.Term,
                    TermAudio = selectedFlashcard.OriginalFlashcard.TermAudio,
                    TermStandardizedForm = selectedFlashcard.OriginalFlashcard.TermStandardizedForm,

                    TermTranslation = selectedFlashcard.TermTranslation,
                    TermTranslationAudio = selectedFlashcard.OriginalFlashcard.TermTranslationAudio,
                    TermStandardizedFormEnglishTranslation = selectedFlashcard.OriginalFlashcard.TermStandardizedFormEnglishTranslation,

                    TermDefinition = selectedFlashcard.OriginalFlashcard.TermDefinition,

                    Context = selectedFlashcard.SentenceExample,
                    ContextAudio = selectedFlashcard.OriginalFlashcard.ContextAudio,
                    ContextTranslation = selectedFlashcard.SentenceExampleTranslation,
                    ContextTranslationAudio = selectedFlashcard.OriginalFlashcard.ContextTranslationAudio,
                    ContextEnglishTranslation = selectedFlashcard.OriginalFlashcard.ContextEnglishTranslation,

                    Type = selectedFlashcard.OriginalFlashcard.Type,
                    ImageCandidates = [selectedFlashcard.SelectedImageRelativePath]
                }
        };

        var deck = new Deck("Preview deck", flashcards);
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

}
