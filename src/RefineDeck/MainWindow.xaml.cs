using RefineDeck.Utils;
using RefineDeck.ViewModels;
using System.Windows;
using System.Windows.Controls;

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
    }

    private async void FlashcardChanged(object sender, SelectionChangedEventArgs e)
    {
        var selectedFlashcard = ViewModel.SelectedFlashcard;
        if (selectedFlashcard is null) return;

        // update WebView preview
        await WebViewHelper.EnsureWebViewIsInitialized(this.Preview);
        // todo: un-hardcode path to previewer
        Preview.Source = new Uri("d:\\Projekty\\FlashcardSpace.Toolkit\\src\\DeckBrowser\\dist\\index.html");


        // Call a JavaScript function with arguments
        var cardPreview = new
        {
            flashcards = new[] {
                new {
                    term = selectedFlashcard.Term,
                    termAudio = "audio/la-casa.mp3",
                    termBaseForm = "la casa",

                    termTranslation = selectedFlashcard.TermTranslation,
                    termTranslationAudio = "audio/dom.mp3",
                    termEnglishTranslation = "a house",

                    termDefinition = "Budynek przeznaczony do zamieszkania przez ludzi, zwłaszcza przez rodzinę lub małą grupę osób.",

                    context = "La casa es grande y bonita.",
                    contextAudio = "audio/la-casa-es-grande-y-bonita.mp3",
                    contextTranslation = "Dom jest duży i piękny.",
                    contextTranslationAudio = "audio/dom-jest-duzy-i-piekny.mp3",
                    contextEnglishTranslation = "The house is big and pretty.",

                    type = "Noun",
                    imageCandidates = new[] {"images/la-casa-01.webp", "images/la-casa-02.webp"}
                }
            }
        };

        var cardPreviewSerialized = System.Text.Json.JsonSerializer.Serialize(cardPreview);
        await Preview.CoreWebView2.ExecuteScriptAsync($"window.setDataFromWpf({cardPreviewSerialized});");

    }
}
