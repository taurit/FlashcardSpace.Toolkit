using CoreLibrary.Models;
using CoreLibrary.Services.AnkiExportService;
using CoreLibrary.Services.GenerativeAiClients.StableDiffusion;
using Microsoft.Extensions.Logging;
using RefineDeck.Utils;
using RefineDeck.ViewModels;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Http;
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
    GeminiQualityAssuranceAgent? QualityAssuranceAgent = null;
    DismissedSuggestionsMemory? DismissedSuggestionsMemory = null;
    ImageGenerator? ImageGenerator = null;

    public MainWindow()
    {
        InitializeComponent();

        var viewModel = new MainWindowViewModel();
        DataContext = viewModel;
        QualityAssuranceAgent = new GeminiQualityAssuranceAgent(viewModel);
        ViewModel.Deck = DeckLoader.LoadDeck();
        ViewModel.PropertyChanged += ViewModel_PropertyChanged;

        DismissedSuggestionsMemory = new DismissedSuggestionsMemory(ViewModel.Deck.DeckPath.DismissedSuggestionsPath);
        InitializeImageGeneratorInstance();
    }

    private void InitializeImageGeneratorInstance()
    {
        // Image generator is needed for the "Improve selected images quality" feature to work
        var httpClient = new HttpClient();
        var imageGeneratorLogger = LoggerFactory
            .Create(builder => builder.AddConsole())
            .CreateLogger<ImageGenerator>();

        // reuse global image cache folder for now; for portability it should be stored in the deck folder, however
        var imageGeneratorSettings = new ImageGeneratorSettings(Parameters.ImageGeneratorCacheFolder);
        ImageGenerator = new ImageGenerator(httpClient, imageGeneratorLogger, imageGeneratorSettings);
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
        if (e.PropertyName == "Term" || e.PropertyName == "TermTranslation" || e.PropertyName == "SentenceExample" || e.PropertyName == "SentenceExampleTranslation")
        {
            UpdateAndPlayAudio(e.PropertyName);
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
    private void UpdateAndPlayAudio_OnClick(object sender, RoutedEventArgs e)
    {
        var tag = ((Button)sender).Tag.ToString();
        UpdateAndPlayAudio(tag);
    }

    private async void UpdateAndPlayAudio(string? tag)
    {
        var audioProvider = AudioPatcher.GetAudioProviderInstance(ViewModel.Deck.DeckPath);

        var card = ViewModel.SelectedFlashcard;
        if (card is null) return;

        switch (tag)
        {
            case "Term":
                var newAudioFilePathTerm = await audioProvider.GenerateAudioOrUseCached(card.Term, ViewModel.Deck.SourceLanguage);
                card.TermAudio = AudioPatcher.ToRelativePath(newAudioFilePathTerm, ViewModel.Deck.DeckPath);
                AudioPlayer.PlayAudio(newAudioFilePathTerm);
                break;
            case "TermTranslation":
                var newAudioFilePathTermTranslation = await audioProvider.GenerateAudioOrUseCached(card.TermTranslation, ViewModel.Deck.TargetLanguage);
                card.TermTranslationAudio = AudioPatcher.ToRelativePath(newAudioFilePathTermTranslation, ViewModel.Deck.DeckPath);
                AudioPlayer.PlayAudio(newAudioFilePathTermTranslation);
                break;
            case "SentenceExample":
                var newAudioFilePathSentenceExample = await audioProvider.GenerateAudioOrUseCached(card.SentenceExample, ViewModel.Deck.SourceLanguage);
                card.SentenceExampleAudio = AudioPatcher.ToRelativePath(newAudioFilePathSentenceExample, ViewModel.Deck.DeckPath);
                AudioPlayer.PlayAudio(newAudioFilePathSentenceExample);
                break;
            case "SentenceExampleTranslation":
                var newAudioFilePathSentenceExampleTranslation = await audioProvider.GenerateAudioOrUseCached(card.SentenceExampleTranslation, ViewModel.Deck.TargetLanguage);
                card.SentenceExampleTranslationAudio = AudioPatcher.ToRelativePath(newAudioFilePathSentenceExampleTranslation, ViewModel.Deck.DeckPath);
                AudioPlayer.PlayAudio(newAudioFilePathSentenceExampleTranslation);

                break;

            default:
                throw new NotImplementedException($"Not implemented: updating the {tag} audio.");
        }


    }

    private enum ScrollDirection { Next, Previous };
    private void ChangeImageOnScroll(object sender, MouseWheelEventArgs e)
    {
        ScrollDirection direction = e.Delta > 0 ? ScrollDirection.Next : ScrollDirection.Previous;

        var card = ViewModel.SelectedFlashcard;
        if (card is null) return;

        // prevent event from being handled by parent control
        e.Handled = true;

        if (card.ImageCandidates.Count == 0) return;

        if (card.SelectedImageIndex is null)
        {
            card.SelectedImageIndex = 0;
            return;
        }

        int newIndex;
        if (direction == ScrollDirection.Next)
        {
            newIndex = card.SelectedImageIndex.Value + 1;
            if (newIndex >= card.ImageCandidates.Count)
                newIndex = card.ImageCandidates.Count - 1;
        }
        else
        {
            newIndex = card.SelectedImageIndex.Value - 1;
            if (newIndex < 0)
                newIndex = 0;
        }

        card.SelectedImageIndex = newIndex;
    }

    private void DismissWarning_OnClick(object sender, RoutedEventArgs e)
    {
        var selectedCard = ViewModel.SelectedFlashcard;
        if (selectedCard is null) return;

        selectedCard.QaSuggestions = "";
    }

    private void DismissSecondOpinionWarning_OnClick(object sender, RoutedEventArgs e)
    {
        var selectedCard = ViewModel.SelectedFlashcard;
        if (selectedCard is null) return;
        if (DismissedSuggestionsMemory is null) return;

        DismissedSuggestionsMemory.Dismiss(selectedCard.QaSuggestionsSecondOpinion);
        selectedCard.QaSuggestionsSecondOpinion = null;
    }

    private async void RunSecondaryQualityAssurance_OnClick(object sender, RoutedEventArgs e)
    {
        if (QualityAssuranceAgent is null) return;
        if (DismissedSuggestionsMemory is null) return;

        ViewModel.PerformingQualityAnalysis = true;
        await QualityAssuranceAgent.ValidateAllCards();

        foreach (var card in ViewModel.Deck.Flashcards.Where(x => DismissedSuggestionsMemory.IsDismissed(x.QaSuggestionsSecondOpinion)))
        {
            card.QaSuggestionsSecondOpinion = new PlainTextAndJsonPart("OK", "OK", null);
        }

        ViewModel.PerformingQualityAnalysis = false;
    }

    private async void UpgradeQualityOfSelectedImages_OnClick(object sender, RoutedEventArgs e)
    {
        if (ImageGenerator is null)
        {
            MessageBox.Show("ImageGenerator is not initialized.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        var needQualityUpgrade = ViewModel.Deck.Flashcards.ToList();
        ViewModel.ProgressBarTotal = needQualityUpgrade.Count;
        ViewModel.ProgressBarProcessed = 0;
        ViewModel.PerformingImageQualityUpgrade = true;

        foreach (var card in needQualityUpgrade)
        {

            if (!card.IsPlaceholderImageSelected)
            {
                var selectedImage = card.SelectedImage!;
                await ImageGenerator.ImproveImageQualityIfNeeded(selectedImage);
            }

            ViewModel.ProgressBarProcessed++;

        }

        ViewModel.PerformingImageQualityUpgrade = false;
    }
}
