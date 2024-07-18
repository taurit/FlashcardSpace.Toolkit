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

    private async void StartReviewFlow_OnClick(object sender, RoutedEventArgs e)
    {
        var conflict = GetNextUnresolvedConflict();

        await SetPreviewWindowHtml(this.LeftPreview, GenerateHtmlPreviewForNote(conflict.Left));
        await SetPreviewWindowHtml(this.RightPreview, GenerateHtmlPreviewForNote(conflict.Right));

    }

    private static string GenerateHtmlPreviewForNote(CardViewModel card)
    {
        // todo Doesn't work, requires serving images via HTTP ... 
        var imagePart = !String.IsNullOrWhiteSpace(card.Note.Image)
            ? $"<br />{card.Note.Image.Replace("src=\"", $"src=\"file:///{Settings.AnkiMediaFolderPath.Replace("\\", "/")}")}"
            : string.Empty;

        var commentsPart = !String.IsNullOrWhiteSpace(card.Note.Comments)
            ? $"<br />{card.Note.Comments}"
            : string.Empty;

        var tagsPart = $"<br />" +
                       $"<span style='font-weight: bold; color: #0000aa;'>{card.Note.NoteTemplateName}</span>" +
                       $"<span style='font-weight: bold; color: #00aa00;'>{card.Note.Tags}</span>";

        return $"<body style='text-align: center; font-size: x-large;'>{card.Question}<hr />{card.Answer}<br /><br />{imagePart}{commentsPart}{tagsPart}</body>";

    }

    private FlashcardConflict GetNextUnresolvedConflict()
    {
        var flashcardsWithConflictOnFront = _flashcards.Where(x =>
            // skip conflict that are already resolved
            !x.Note.IsScheduledForRemoval &&
            x.DuplicatesOfQuestion.Count(dup => !dup.Note.IsScheduledForRemoval) > 0
         ).ToList();

        var flashcard = flashcardsWithConflictOnFront.First();
        var conflictingFlashcards = flashcard.DuplicatesOfQuestion.Where(dup => !dup.Note.IsScheduledForRemoval);
        var firstConflictingFlashcard = conflictingFlashcards.First();

        return new FlashcardConflict(flashcard, firstConflictingFlashcard);
    }
}

internal record FlashcardConflict(CardViewModel Left, CardViewModel Right);
