using AnkiCardValidator.Models;
using AnkiCardValidator.Utilities;
using AnkiCardValidator.ViewModels;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Windows;

namespace AnkiCardValidator;

public partial class MainWindow : Window
{
    MainWindowViewModel ViewModel => (MainWindowViewModel)this.DataContext;

    readonly NormalFormProvider _normalFormProvider = new();

    readonly FrequencyDataProvider _spanishFrequencyDataProvider;
    readonly FrequencyDataProvider _polishFrequencyDataProvider;
    readonly DuplicateDetector _duplicateDetector;
    readonly DefinitionCounter _definitionCounter;
    readonly FlashcardDirectionDetector _directionDetector;

    public MainWindow()
    {
        InitializeComponent();
        this.DataContext = new MainWindowViewModel();

        // tech debt: DI container should make it simpler
        _duplicateDetector = new(_normalFormProvider);
        _definitionCounter = new();
        _spanishFrequencyDataProvider = new(_normalFormProvider, Settings.FrequencyDictionarySpanish);
        _polishFrequencyDataProvider = new(_normalFormProvider, Settings.FrequencyDictionaryPolish);
        _directionDetector = new(_normalFormProvider, _polishFrequencyDataProvider, _spanishFrequencyDataProvider);
    }

    private async void LoadFlashcards_OnClick(object sender, RoutedEventArgs e)
    {
        var sw = Stopwatch.StartNew();

        var notes = AnkiHelpers.GetAllNotesFromSpecificDeck(Settings.AnkiDatabaseFilePath, "1. Spanish", null);
        ViewModel.Flashcards.Clear();

        foreach (var note in notes)
        {
            var cardsViewModel = CreateCardsForNote(note, notes);

            foreach (var cardViewModel in cardsViewModel)
            {
                ViewModel.Flashcards.Add(cardViewModel);
            }
        }

        // handle duplicates
        foreach (var card in ViewModel.Flashcards)
        {
            var duplicatesOfQuestion = _duplicateDetector.DetectDuplicatesInQuestion(card, ViewModel.Flashcards);
            card.DuplicatesOfQuestion.Clear();
            foreach (var duplicate in duplicatesOfQuestion) { card.DuplicatesOfQuestion.Add(duplicate); }
        }


        await ReloadFlashcardsEvaluationAndSortByMostPromising();

        sw.Stop();
        ViewModel.StatusMessage = $"Loaded {ViewModel.Flashcards.Count} flashcards in {sw.ElapsedMilliseconds} ms.";
    }

    private List<CardViewModel> CreateCardsForNote(AnkiNote note, List<AnkiNote> notes)
    {
        if (note.NoteTemplateName != "OneDirection" && note.NoteTemplateName != "BothDirections")
            throw new InvalidOperationException($"Unexpected note template name: {note.NoteTemplateName}");

        List<CardViewModel> cards = new();

        var noteDirection = _directionDetector.DetectDirectionOfACard(note);

        var frequencyPositionFrontSide = (noteDirection == FlashcardDirection.FrontTextInPolish)
                        ? _polishFrequencyDataProvider.GetPosition(note.FrontText)
                        : _spanishFrequencyDataProvider.GetPosition(note.FrontText);
        var frequencyPositionBackSide = (noteDirection == FlashcardDirection.FrontTextInPolish)
                        ? _spanishFrequencyDataProvider.GetPosition(note.BackText)
                        : _polishFrequencyDataProvider.GetPosition(note.BackText);
        var numDefinitionsOnFrontSide = _definitionCounter.CountDefinitions(note.FrontText);
        var numDefinitionsOnBackSide = _definitionCounter.CountDefinitions(note.BackText);

        var basicCard = new CardViewModel(note, false, noteDirection, frequencyPositionFrontSide, frequencyPositionBackSide, numDefinitionsOnFrontSide, numDefinitionsOnBackSide, CefrClassification.Unknown, null, null);

        cards.Add(basicCard);

        if (note.NoteTemplateName == "BothDirections")
        {
            var reverseCard = new CardViewModel(note, true, noteDirection, frequencyPositionBackSide, frequencyPositionFrontSide, numDefinitionsOnBackSide, numDefinitionsOnFrontSide, CefrClassification.Unknown, null, null);

            cards.Add(reverseCard);
        }

        return cards;
    }


    private async void EvaluateFewMoreCards_OnClick(object sender, RoutedEventArgs e)
    {
        // should be large enough to reduce cost overhead of long prompt, but not too big to avoid timeouts
        const int chunkSize = 29;

        var numCardsToEvaluate = Int32.Parse(NumCardsToEvaluate.Text);

        // Load as much as you can from the local cache
        var vmsForWhichCacheExists = ViewModel.Flashcards.Where(x => File.Exists(GenerateCacheFilePath(x))).ToList();

        // Sort the rest by the most promising ones (lowest penalty score)
        var allFlashcardsWithEvaluationMissing = ViewModel.Flashcards.Except(vmsForWhichCacheExists);
        var flashcardsToEvaluateThisTime = allFlashcardsWithEvaluationMissing
            .Where(x => !x.IsQuestionInPolish) // todo currently my ChatGpt query only works for the Spanish->Polish direction
            .OrderBy(x => x.Penalty)
            .Take(numCardsToEvaluate)
            .ToList();

        var chunksToEvaluate = flashcardsToEvaluateThisTime.Chunk(chunkSize).ToList();

        var jsonSerializerOptions = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true
        };

        foreach (var chunk in chunksToEvaluate)
        {
            // sanity check 
            if (chunk.Any(x => x.IsQuestionInPolish))
            {
                throw new InvalidOperationException("All cards in the chunk must be in Spanish->Polish direction.");
            }

            var chunkOfNotes = chunk.Select(x => new FlashcardToEvaluateSpanishToPolish(x.Question, x.Answer)).ToList();
            var evaluationResult = await FlashcardQualityEvaluator.EvaluateFlashcardsQuality(chunkOfNotes);

            int i = 0;

            foreach (var vm in chunk)
            {
                var evaluation = evaluationResult.Evaluations[i];

                // save to cache
                var cacheFilePath = GenerateCacheFilePath(vm);
                var cacheContent = new FlashcardQualityEvaluationCacheModel(evaluation, evaluationResult.RawChatGptResponse);
                var cacheContentSerialized = JsonSerializer.Serialize(cacheContent, jsonSerializerOptions);
                await File.WriteAllTextAsync(cacheFilePath, cacheContentSerialized);

                i++;
            }
        }

        await ReloadFlashcardsEvaluationAndSortByMostPromising();
    }

    private async Task ReloadFlashcardsEvaluationAndSortByMostPromising()
    {
        foreach (var vm in ViewModel.Flashcards)
        {
            await TryUpdateViewModelWithEvaluationData(vm);
        }

        var cardsByBestQuality = ViewModel.Flashcards
            .OrderBy(x => x.Penalty)
            .ThenBy(x => x.Question)
            ;
        ViewModel.Flashcards = new ObservableCollection<CardViewModel>(cardsByBestQuality);
    }

    private static string GenerateCacheFilePath(CardViewModel cardVm)
    {
        var cardId = cardVm.IsQuestionInPolish
            ? $"pl{cardVm.Note.Id}"
            : $"{cardVm.Note.Id}"  // for Spanish, just use the note ID to not discard existing cache which used note id only
            ;

        var cacheFileName = $"eval-{Settings.OpenAiModelId}_{cardId}.txt";
        var cacheFilePath = Path.Combine(Settings.GptResponseCacheDirectory, cacheFileName);
        return cacheFilePath;
    }

    private static async Task TryUpdateViewModelWithEvaluationData(CardViewModel cardVm)
    {
        var cacheFilePath = GenerateCacheFilePath(cardVm);
        if (!File.Exists(cacheFilePath)) return;

        var cacheContent = await File.ReadAllTextAsync(cacheFilePath);

        var cached = JsonSerializer.Deserialize<FlashcardQualityEvaluationCacheModel>(cacheContent);

        if (cached is null)
        {
            throw new InvalidOperationException("Failed to deserialize cache content.");
        }

        cardVm.CefrLevelQuestion = cached.Evaluation.CEFR;
        cardVm.QualityIssues = cached.Evaluation.Issues;
        cardVm.RawResponseFromChatGptApi = cached.RawChatGptResponse;

        cardVm.Meanings.Clear();
        foreach (var meaning in cached.Evaluation.Meanings)
        {
            cardVm.Meanings.Add(meaning);
        }
    }

    private async void TagGreenCards_OnClick(object sender, RoutedEventArgs e)
    {
        var cardsWithAcceptablePenalty = ViewModel.Flashcards
            .Where(x => x.Penalty <= 1) // change to 0 for perfect cards
            .ToList();
        AnkiHelpers.AddTagToNotes(Settings.AnkiDatabaseFilePath, cardsWithAcceptablePenalty, "modified");

        await ReloadFlashcardsEvaluationAndSortByMostPromising();
    }

    private async void ResolveDuplicates_OnClick(object sender, RoutedEventArgs e)
    {
        // we can skip cards with no duplicates for performance; they won't be needed in the flow
        var flashcardsWithDuplicates = ViewModel.Flashcards
            .Where(x => x.DuplicatesOfQuestion.Count > 0)
            .ToList();

        var resolveDuplicatesTool = new ResolveDuplicatesTool(flashcardsWithDuplicates);
        resolveDuplicatesTool.ShowDialog();

        await ReloadFlashcardsEvaluationAndSortByMostPromising();
    }
}

internal record FlashcardQualityEvaluationCacheModel(FlashcardQualityEvaluation Evaluation, string RawChatGptResponse);
