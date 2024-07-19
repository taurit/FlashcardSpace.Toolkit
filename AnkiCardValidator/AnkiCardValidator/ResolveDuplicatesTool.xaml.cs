using AnkiCardValidator.Utilities;
using AnkiCardValidator.ViewModels;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using System.Windows;

namespace AnkiCardValidator;

/// <summary>
/// Interaction logic for ResolveDuplicatesTool.xaml
/// </summary>
public partial class ResolveDuplicatesTool : Window
{
    private readonly List<CardViewModel> _flashcards;

    public ResolveDuplicatesTool(List<CardViewModel> flashcards)
    {
        _flashcards = flashcards;
        InitializeComponent();
    }

    private static async Task SetPreviewWindowHtml(WebView2 webViewControl, string htmlContentToSet)
    {
        // Ensure that CoreWebView2 is initialized
        if (webViewControl.Source is null || webViewControl.Tag is null)
        {
            var options =
                new CoreWebView2EnvironmentOptions(
                    "--disable-web-security --allow-file-access-from-files --allow-file-access --autoplay-policy=no-user-gesture-required"); // to allow load images and audio files from the disk
            var environment = await CoreWebView2Environment.CreateAsync(null, null, options);
            await webViewControl.EnsureCoreWebView2Async(environment);
            webViewControl.Tag = true;
        }

        webViewControl.NavigateToString(htmlContentToSet);
    }

    FlashcardConflict? _currentConflict = null;
    private async void StartReviewFlow_OnClick(object sender, RoutedEventArgs e)
    {
        await ProgressToNextUnresolvedConflict();
    }

    private async Task ProgressToNextUnresolvedConflict()
    {
        _currentConflict = GetNextUnresolvedConflict();

        if (_currentConflict is null) return;

        await SetPreviewWindowHtml(this.LeftPreview, GenerateHtmlPreviewForNote(_currentConflict.Left));
        await SetPreviewWindowHtml(this.RightPreview, GenerateHtmlPreviewForNote(_currentConflict.Right));
    }

    private static string GenerateHtmlPreviewForNote(CardViewModel card)
    {
        // todo Doesn't work, requires serving images via HTTP ... 
        var imagePart = !String.IsNullOrWhiteSpace(card.Note.Image)
            ? $"<br />{card.Note.Image.Replace("src=\"", $"src=\"http://localhost:3000/")}"
            : string.Empty;

        var questionAudioPart = !String.IsNullOrWhiteSpace(card.QuestionAudio)
            ? ConvertAnkiAudioTagToHtmlAudioTag(card.QuestionAudio)
            : "";

        var answerAudioPart = !String.IsNullOrWhiteSpace(card.AnswerAudio)
            ? ConvertAnkiAudioTagToHtmlAudioTag(card.AnswerAudio)
            : "";

        var commentsPart = !String.IsNullOrWhiteSpace(card.Note.Comments)
            ? $"<br />{card.Note.Comments}"
            : string.Empty;

        var issuesPart = !String.IsNullOrWhiteSpace(card.QualityIssues)
            ? $"<br /><span style='font-weight: bold; color: #ff0000;'><b>Issues</b>: {card.QualityIssues}</span>"
            : string.Empty;

        var tagsPart = $"<br />" +
                       $"<span style='font-weight: bold; color: #0000aa;'>{card.Note.NoteTemplateName}</span>" +
                       issuesPart +
                       $"<span style='font-weight: bold; color: #00aa00;'>{card.Note.Tags}</span>" +
                       $"<br /><span style='font-weight: bold; color: #eeaa00;'>answer's position in frequency dictionary: {card.FrequencyPositionAnswer}</span>";

        var styles = "<style type='text/css'>" +
                     "img { max-height: 400px; max-width: 600px; }" +
                     "</style>";

        return $"<head>{styles}</head><body style='text-align: center; font-size: x-large;'>{card.Question}{questionAudioPart}<hr />{card.Answer}{answerAudioPart}<br />{imagePart}{commentsPart}{tagsPart}</body>";

    }

    /// <summary>
    /// Converts Anki audio tag (example: `[sound:file.mp3]`) to HTML audio tag allowing to play the mp3 file.
    /// 
    /// </summary>
    /// <param name="ankiAudioTag"></param>
    private static string ConvertAnkiAudioTagToHtmlAudioTag(string ankiAudioTag)
    {
        string audioPlayerHtmlCodeFragment = "<audio controls>" +
                                             "<source src=\"http://localhost:3000/" + ankiAudioTag.Replace("[sound:", "").Replace("]", "") + "\" type=\"audio/mpeg\">" +
                                             "</audio>";


        return $"<br />{audioPlayerHtmlCodeFragment}";
    }

    private readonly List<FlashcardConflict> _transientlySkippedConflicts = new();

    private FlashcardConflict? GetNextUnresolvedConflict()
    {
        var flashcardsWithConflictOnFront = _flashcards.Where(x =>
            // skip conflict that are already resolved
            !x.Note.IsScheduledForRemoval &&
            !x.Note.IsScheduledForManualResolution &&
            _transientlySkippedConflicts.All(t => t.Left != x && t.Right != x) &&
            x.DuplicatesOfQuestion.Count(dup => !dup.Note.IsScheduledForRemoval) > 0
         ).ToList();

        StatusBarText.Text = $"Conflicts left to resolve: {flashcardsWithConflictOnFront.Count / 2}";

        var flashcard = flashcardsWithConflictOnFront.FirstOrDefault();
        if (flashcard is null)
        {
            MessageBox.Show("No more conflicts to resolve.");
            Close();
            return null;
        }
        var conflictingFlashcards = flashcard.DuplicatesOfQuestion.Where(dup => !dup.Note.IsScheduledForRemoval);
        var firstConflictingFlashcard = conflictingFlashcards.First();

        return new FlashcardConflict(flashcard, firstConflictingFlashcard);
    }

    private async void KeepLeft_OnClick(object sender, RoutedEventArgs e)
    {
        if (_currentConflict is null)
            throw new InvalidOperationException("Start the review flow first; no conflict to resolve yet");

        AnkiHelpers.AddTagToNotes(Settings.AnkiDatabaseFilePath, [_currentConflict.Right], "toDelete");

        await ProgressToNextUnresolvedConflict();
    }

    private async void KeepRight_OnClick(object sender, RoutedEventArgs e)
    {
        if (_currentConflict is null)
            throw new InvalidOperationException("Start the review flow first; no conflict to resolve yet");

        AnkiHelpers.AddTagToNotes(Settings.AnkiDatabaseFilePath, [_currentConflict.Left], "toDelete");

        await ProgressToNextUnresolvedConflict();
    }

    private async void DecideLater_OnClick(object sender, RoutedEventArgs e)
    {
        if (_currentConflict is null)
            throw new InvalidOperationException("Start the review flow first; no conflict to resolve yet");

        _transientlySkippedConflicts.Add(_currentConflict);
        await ProgressToNextUnresolvedConflict();
    }

    private async void ResolveManually_OnClick(object sender, RoutedEventArgs e)
    {
        AnkiHelpers.AddTagToNotes(Settings.AnkiDatabaseFilePath, [_currentConflict!.Left, _currentConflict.Right], "toResolveManually");
        await ProgressToNextUnresolvedConflict();
    }
}

internal record FlashcardConflict(CardViewModel Left, CardViewModel Right);

