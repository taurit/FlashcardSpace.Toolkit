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
        if (webViewControl.Source is null)
        {
            var options = new CoreWebView2EnvironmentOptions();
            var environment = await CoreWebView2Environment.CreateAsync(null, null, options);
            await webViewControl.EnsureCoreWebView2Async(environment);
        }

        webViewControl.NavigateToString(htmlContentToSet);
    }

    private async void StartReviewFlow_OnClick(object sender, RoutedEventArgs e)
    {
        var conflict = GetNextUnresolvedConflict();

        await SetPreviewWindowHtml(this.LeftPreview, GenerateHtmlPreviewForNote(conflict.Left));
        await SetPreviewWindowHtml(this.RightPreview, GenerateHtmlPreviewForNote(conflict.Right));

    }

    private static string GenerateHtmlPreviewForNote(AnkiNote note)
    {
        return $"{note.FrontSide}<hr />{note.BackSide}";
    }

    private FlashcardConflict GetNextUnresolvedConflict()
    {
        var flashcardsWithConflictOnFront = _flashcards.Where(x =>
            // skip conflict that are already resolved
            !x.Note.IsScheduledForRemoval &&
            x.DuplicatesOfQuestion.Count(dup => !dup.IsScheduledForRemoval) > 0
         ).ToList();

        // todo add support for conflicts in the back side

        var flashcard = flashcardsWithConflictOnFront.First();
        var conflictingFlashcards = flashcard.DuplicatesOfQuestion.Where(dup => !dup.IsScheduledForRemoval);
        var firstConflictingFlashcard = conflictingFlashcards.First();

        return new FlashcardConflict(flashcard.Note, firstConflictingFlashcard);
    }
}

internal record FlashcardConflict(AnkiNote Left, AnkiNote Right);
