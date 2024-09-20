using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;

namespace RefineDeck.Utils;
internal class WebViewHelper
{
    public static async Task EnsureWebViewIsInitialized(WebView2 webView)
    {
        if (webView.Source is null || webView.Tag is null)
        {
            var options =
                new CoreWebView2EnvironmentOptions(
                    "--disable-web-security --allow-file-access-from-files --allow-file-access --autoplay-policy=no-user-gesture-required"); // to allow load images and audio files from the disk
            var environment = await CoreWebView2Environment.CreateAsync(null, null, options);
            await webView.EnsureCoreWebView2Async(environment);
            webView.Tag = true;
        }

    }
}
