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
    private readonly List<FlashcardViewModel> _flashcards;

    public ResolveDuplicatesTool(List<FlashcardViewModel> flashcards)
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
        await SetPreviewWindowHtml(this.LeftPreview, "Yo <b>dawg</b>");
    }

    private async void GetNextUnresolvedConflict(object sender, RoutedEventArgs e)
    {
        var flashcardsWithConflictOnFront = _flashcards.Where(x => x.DuplicatesOfFrontSide.Count > 0).ToList();
        // todo add support for conflicts in the back side

    }
}
