using BookToAnki.Models;
using BookToAnki.NotePropertiesDatabase;
using BookToAnki.Services;
using BookToAnki.Services.OpenAi;
using BookToAnki.UI.Components;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using System.Threading.Tasks;
using System.Windows;

namespace BookToAnki.UI.Features;

public record AnkiCardPreviewWindowContext(Window ParentWindow, UkrainianAnkiNote Note);

public static class WebView2Extensions
{
    internal static OpenAiServiceWrapper? OpenAiService;
    internal static AudioExampleProvider? AudioExampleProvider;
    internal static NoteProperties? NoteProperties;

    public static async Task SetPreviewWindowHtml(this WebView2 webViewComponent, string htmlContent,
        AnkiCardPreviewWindowContext context, OpenAiServiceWrapper openAiService, NoteProperties noteProperties, AudioExampleProvider audioExampleProvider)
    {
        // Ensure that CoreWebView2 is initialized
        if (webViewComponent.Source is null)
        {
            WebView2Extensions.OpenAiService = openAiService;
            WebView2Extensions.NoteProperties = noteProperties;
            WebView2Extensions.AudioExampleProvider = audioExampleProvider;

            var options =
                new CoreWebView2EnvironmentOptions(
                    "--disable-web-security --allow-file-access-from-files --allow-file-access --autoplay-policy=no-user-gesture-required"); // to allow load audio files
            var environment = await CoreWebView2Environment.CreateAsync(null, null, options);
            var s = environment.FailureReportFolderPath;

            await webViewComponent.EnsureCoreWebView2Async(environment);
            webViewComponent.WebMessageReceived += NoteRatingFlow.WebViewComponentOnWebMessageReceived;
        }

        webViewComponent.Tag = context;
        webViewComponent.NavigateToString(htmlContent);
    }

}
