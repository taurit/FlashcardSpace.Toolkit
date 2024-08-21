using BookToAnki.Models;
using BookToAnki.NotePropertiesDatabase;
using BookToAnki.Services;
using BookToAnki.Services.OpenAi;
using BookToAnki.UI.ViewModels;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace BookToAnki.UI.Components;

public partial class NoteRatingFlow : Window
{
    // just a guess that Garbage Collection kills this window, so I assign it to variable - and it seems to solve the issue
    private static TextInputDialog? _textInputDialog;
    private static SoundEditorDialog? _soundEditorDialog;
    private static SentenceTranslationDialog? _sentenceTranslationDialog;

    private readonly List<WordUsageExample> _usagesToRate = new();
    private List<AlreadyRatedCard> _alreadyRated;
    private int _currentIndex;
    private List<WordData> _words;

    private readonly AnkiNoteGenerator _ankiNoteGenerator;
    private readonly NoteProperties _noteProperties;
    private readonly AudioExampleProvider _audioExampleProvider;
    private readonly OpenAiServiceWrapper _openAiService;

    public NoteRatingFlow(NoteProperties noteProperties, AnkiNoteGenerator ankiNoteGenerator,
        OpenAiServiceWrapper openAiService, AudioExampleProvider audioExampleProvider)
    {
        _noteProperties = noteProperties;
        _ankiNoteGenerator = ankiNoteGenerator;
        _openAiService = openAiService;
        _audioExampleProvider = audioExampleProvider;

        InitializeComponent();
    }

    private WordUsageExample CurrentUsage => _usagesToRate[_currentIndex];
    private PrefKey CurrentKey => new(CurrentUsage.Word, CurrentUsage.Sentence.Text);

    // ReSharper disable once MemberCanBePrivate.Global
    public CardRatingFlowViewModel ViewModel { get; set; }

    public void Display(ObservableCollection<WordDataViewModel> words)
    {
        _alreadyRated = _noteProperties.GetAlreadyRated();

        // don't spend time on examples for which perfect examples are already selected
        var wordsAlreadyReviewed = words.Where(x =>
            _alreadyRated.Count(z => z.PrefKey.Word == x.Word.Word && z.Rating == Rating.Premium) >= Settings.NumPerfectExamplesToConsiderCardDone).ToList();

        _words = words
            // let's focus on nouns first, as it can be a separate product
            .Where(x => x.PartOfSpeech == "nouns" || x.PartOfSpeech == "verbs")
            .Except(wordsAlreadyReviewed)
            .Select(x => x.Word)
            .ToList();

        ViewModel = new CardRatingFlowViewModel();
        ViewModel.NumExamplesRatedOnFlowStart = _alreadyRated.Count;
        DataContext = ViewModel;

        // Show, not ShowDialog, so I can minimize main window and reduce distraction on the screen while in the flow
        Show();
    }

    private async void PopulateUsagesToRate()
    {
        foreach (var word in _words)
            // take the most promising usage examples
            foreach (var usage in word.UsageExamples
                         // discard own names, like Петунія
                         .Where(x => x.Word.ToLowerInvariant() == x.Word)
                         // discard ones with no audio
                         .Where(x => x.TranscriptMatches.Any())
                         // discard ones which are already rated
                         .Where(x => _alreadyRated.All(z => z.PrefKey != new PrefKey(x.Word, x.Sentence.Text)))
                         // I strive for 2 (or... Settings.NumPerfectExamplesToConsiderCardDone) perfect examples, so take a bit more to have room for filtering out
                         .Take(Settings.NumPerfectExamplesToConsiderCardDone + 1)
                    )
                _usagesToRate.Add(usage);

        // one time job, needed after I update prompt: prime cache of GPT4o-mini sentence translations. Cheap and makes work more comfortable
        if (true)
        {
            var sentences = _usagesToRate.Select(x => new { x.Sentence.Text, x.Word }).ToList();
            foreach (var sentence in sentences)
            {
                var prompt = SentenceTranslationDialog.PreparePrompt(sentence.Text, sentence.Word);
                var completion = await _openAiService.CreateChatCompletion(prompt.SystemPrompt, prompt.UserPrompt, OpenAI.ObjectModels.Models.Gpt_4o_mini, false);
                Debug.WriteLine($"Primed cache for explaining {sentence.Text}, {sentence.Word} with GPT4o-mini.");
            }
        }

        ViewModel.TotalNumCardsToRate = _usagesToRate.Count + _alreadyRated.Count;
    }

    // Workaround
    // I tried initializing HTML component OnInitialized, but it freezes and renders black window.
    // I can't in the constructor because it cant be `async Task` or `async void`.
    private async void Start_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            PopulateUsagesToRate();

            (sender as Button).Visibility = Visibility.Hidden;

            await RenderSelectedCard();
        }
        catch (Exception ex)
        {
            // Handle or log the exception
            Debug.WriteLine(ex.ToString());
            MessageBox.Show(ex.ToString());
        }
    }

    private async Task RenderSelectedCard()
    {
        try
        {
            var note = await _ankiNoteGenerator.GenerateAnkiNote(CurrentUsage, Settings.ImagesRepositoryFolder);
            var ankiCardPreviewWindowContext = new AnkiCardPreviewWindowContext(this, note);
            await CardPreviewRating.SetPreviewWindowHtml(note.UkrainianToPolishCard.PreviewHtml,
                ankiCardPreviewWindowContext, _openAiService, _noteProperties, _audioExampleProvider);
            ViewModel.CurrentRating = note.Rating;
        }
        catch (Exception ex)
        {
            // Handle or log the exception
            Debug.WriteLine(ex.ToString());
            MessageBox.Show(ex.ToString());
        }
    }

    private void Unacceptable_OnClick(object sender, RoutedEventArgs e)
    {
        RateCurrentNote(Rating.Rejected); // async!
    }

    private void Acceptable_OnClick(object sender, RoutedEventArgs e)
    {
        RateCurrentNote(Rating.AcceptableForPragmatics); // async!
    }

    private void Premium_OnClick(object sender, RoutedEventArgs e)
    {
        RateCurrentNote(Rating.Premium); // async!
    }

    private async void RateCurrentNote(Rating rating)
    {
        try
        {
            _noteProperties.SetNoteRating(CurrentKey, rating);
            _alreadyRated.Add(new AlreadyRatedCard(CurrentKey, rating));

            var justRated = CurrentKey;

            var wordHasEnoughPerfectExamples =
                _alreadyRated.Count(z => z.PrefKey.Word == CurrentKey.Word && z.Rating == Rating.Premium) >= Settings.NumPerfectExamplesToConsiderCardDone;
            if (!wordHasEnoughPerfectExamples)
                _currentIndex++;
            else
                // skip also cards that don't need rating if we have enough perfect samples
                while (CurrentKey.Word == justRated.Word)
                    _currentIndex++;

            ViewModel.CurrentCardIndex = _currentIndex;
            await RenderSelectedCard();
        }
        catch (Exception ex)
        {
            // Handle or log the exception
            Debug.WriteLine(ex.ToString());
            MessageBox.Show(ex.ToString());
        }
    }


    private async void Back_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            _currentIndex--;
            if (_currentIndex < 0) _currentIndex = 0;
            await RenderSelectedCard();
        }
        catch (Exception ex)
        {
            // Handle or log the exception
            Debug.WriteLine(ex.ToString());
            MessageBox.Show(ex.ToString());
        }
    }

    private async void Forward_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_currentIndex >= _usagesToRate.Count - 1)
            {
                MessageBox.Show("No more cards to rate in the queue");
            }
            else
            {
                _currentIndex++;
                await RenderSelectedCard();
            }
        }
        catch (Exception ex)
        {
            // Handle or log the exception
            Debug.WriteLine(ex.ToString());
            MessageBox.Show(ex.ToString());
        }
    }

    public static void WebViewComponentOnWebMessageReceived(object? sender,
        CoreWebView2WebMessageReceivedEventArgs e)
    {
        var fieldToEditString = e.TryGetWebMessageAsString();
        var fieldToEditEnum = Enum.Parse<EditableField>(fieldToEditString);
        var skipCardReRendering = false;

        var cardContext = (sender as WebView2)!.Tag as AnkiCardPreviewWindowContext;
        if (cardContext is null)
            throw new InvalidOperationException(
                "Unexpected: WebView2 should always have a context describing the note as a Tag.");

        var noteProperties = WebView2Extensions.NoteProperties!;
        var openAiService = WebView2Extensions.OpenAiService!;
        var audioExampleProvider = WebView2Extensions.AudioExampleProvider!;

        SynchronizationContext.Current!.Post(async _ =>
        {
            var parentWindow = cardContext!.ParentWindow;
            var originalNote = cardContext.Note;

            switch (fieldToEditEnum)
            {
                case EditableField.OriginalNominative:
                    var newValueNom = ReadText(cardContext, fieldToEditEnum, originalNote.NominativeForm);
                    if (newValueNom is not null)
                    {
                        noteProperties.SetWordNominativeOriginal(originalNote.PrefKey, newValueNom.Value);
                        if (newValueNom.ScopeToWord)
                        {
                            var wordKey = ChangeKeyScopeToWord(originalNote.PrefKey);
                            noteProperties.SetWordNominativeOriginal(wordKey, newValueNom.Value);
                        }
                    }
                    else
                    {
                        skipCardReRendering = true;
                    }

                    break;

                case EditableField.PolishWordTranslation:
                    var newValuePl = ReadText(cardContext, fieldToEditEnum, originalNote.BestWordEquivalentInPolish);
                    if (newValuePl is not null)
                    {
                        noteProperties.SetWordNominativePl(originalNote.PrefKey, newValuePl.Value);
                        if (newValuePl.ScopeToWord)
                        {
                            var wordKey = ChangeKeyScopeToWord(originalNote.PrefKey);
                            noteProperties.SetWordNominativePl(wordKey, newValuePl.Value);
                        }
                    }
                    else
                    {
                        skipCardReRendering = true;
                    }

                    break;

                case EditableField.PolishSentenceTranslation:
                    var newValueSePl = ReadSentenceTranslation(noteProperties, openAiService, cardContext, fieldToEditEnum,
                        originalNote,
                        originalNote.PrefKey.Word);
                    if (newValueSePl is not null)
                    {
                        noteProperties.SetSentenceTranslationPl(originalNote.PrefKey, newValueSePl.Value);
                        if (newValueSePl.ScopeToWord)
                        {
                            var wordKey = ChangeKeyScopeToWord(originalNote.PrefKey);
                            noteProperties.SetSentenceTranslationPl(wordKey, newValueSePl.Value);
                        }
                    }
                    else
                    {
                        skipCardReRendering = true;
                    }

                    break;

                case EditableField.PolishWordExplanation:
                    var newValueExp = ReadText(cardContext, fieldToEditEnum, originalNote.WordExplanationInPolish);
                    if (newValueExp is not null)
                    {
                        noteProperties.SetWordExplanationPl(originalNote.PrefKey, newValueExp.Value);
                        if (newValueExp.ScopeToWord)
                        {
                            var wordKey = ChangeKeyScopeToWord(originalNote.PrefKey);
                            noteProperties.SetWordExplanationPl(wordKey, newValueExp.Value);
                        }
                    }
                    else
                    {
                        skipCardReRendering = true;
                    }

                    break;

                case EditableField.SelectedAudioTrim:
                    var audioShift = await ReadAudioShift(audioExampleProvider, cardContext);
                    if (audioShift is not null)
                    {
                        noteProperties.SetAudioSampleShift(originalNote.PrefKey, audioShift);

                        // invalidate cache
                        var cachedAudioFragment = Path.Combine(Settings.AudioFilesCacheFolder,
                            originalNote.UkrainianSentenceAudioFileName);
                        if (File.Exists(cachedAudioFragment))
                            File.Delete(cachedAudioFragment);
                    }

                    skipCardReRendering = true;

                    break;
            }

            if (parentWindow is NoteRatingFlow flowWindow && !skipCardReRendering)
                await flowWindow.RenderSelectedCard();
        }, null);
    }

    private static async Task<AudioShift?> ReadAudioShift(AudioExampleProvider audioExampleProvider, AnkiCardPreviewWindowContext context)
    {
        var padding = TimeSpan.FromSeconds(2);
        var shift = new AudioShift(-padding, padding);
        var paddedAudioFileName = await audioExampleProvider.GenerateAudioSample(context.Note.SelectedAudioFile, shift);
        var fullPathToAudioFile = Path.Combine(Settings.AudioFilesCacheFolder, paddedAudioFileName);
        _soundEditorDialog = new SoundEditorDialog(fullPathToAudioFile, padding);
        _soundEditorDialog.WindowStartupLocation = WindowStartupLocation.Manual;
        _soundEditorDialog.Left = context.ParentWindow.Left + (context.ParentWindow.Width - _soundEditorDialog.Width) / 2;
        _soundEditorDialog.Top = context.ParentWindow.Top + (context.ParentWindow.Height - _soundEditorDialog.Height) / 2;

        if (_soundEditorDialog.ShowDialog() == true)
            return _soundEditorDialog.Shift;

        return null;
    }

    private static PrefKey ChangeKeyScopeToWord(PrefKey key)
    {
        return key with { SentenceExample = "*" };
    }

    private static ValueOverride? ReadText(AnkiCardPreviewWindowContext context, EditableField field,
        string? currentValue)
    {
        _textInputDialog = new TextInputDialog($"Please enter new value for {field}", currentValue);
        _textInputDialog.WindowStartupLocation = WindowStartupLocation.Manual;
        _textInputDialog.Left = context.ParentWindow.Left + (context.ParentWindow.Width - _textInputDialog.Width) / 2;
        _textInputDialog.Top = context.ParentWindow.Top + (context.ParentWindow.Height - _textInputDialog.Height) / 2;

        if (_textInputDialog.ShowDialog() == true)
            return new ValueOverride(_textInputDialog.ResponseText, _textInputDialog.ScopeToWord);

        return null;
    }

    private static ValueOverride? ReadSentenceTranslation(NoteProperties noteProperties, OpenAiServiceWrapper openAiService, AnkiCardPreviewWindowContext context, EditableField field,
        UkrainianAnkiNote note, string wordOriginalForm)
    {
        var widerContext =
            $"{note.SelectedAudioFile.Sentence.PreviousSentence?.Text} {note.SelectedAudioFile.Sentence.Text} {note.SelectedAudioFile.Sentence.NextSentence?.Text}";

        var customTranslation = noteProperties.GetSentenceTranslationPl(note.PrefKey);
        var defaultAnswer = customTranslation ??
                            note.SentenceMachineTranslationToPl ?? note.SentenceEquivalentInPolish ?? "";

        _sentenceTranslationDialog = new SentenceTranslationDialog($"Please enter new value for {field}",
            defaultAnswer,
            note.SentenceEquivalentInPolish,
            note.SentenceMachineTranslationToPl,
            widerContext,
            context.Note.PrefKey.SentenceExample,
            openAiService,
            wordOriginalForm);
        _sentenceTranslationDialog.WindowStartupLocation = WindowStartupLocation.Manual;
        _sentenceTranslationDialog.Left = context.ParentWindow.Left + (context.ParentWindow.Width - _sentenceTranslationDialog.Width) / 2;
        _sentenceTranslationDialog.Top = context.ParentWindow.Top + (context.ParentWindow.Height - _sentenceTranslationDialog.Height) / 2;

        if (_sentenceTranslationDialog.ShowDialog() == true)
            return new ValueOverride(_sentenceTranslationDialog.UserProvidedSentenceTranslation, _sentenceTranslationDialog.ScopeToWord);

        return null;
    }


    private record ValueOverride(string? Value, bool ScopeToWord);
}
