using BookToAnki.Interfaces;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace BookToAnki.Services;

public partial class UkrainianStressHighlighter : IUkrainianStressHighlighter
{
    private static readonly HttpClient HttpClient = new();

    public async Task<string?> HighlightStresses(string inputText)
    {
        // Sending GET request to the specified URL
        var response =
            await HttpClient.GetAsync($"https://slovnyk.ua/nagolos.php?text={Uri.EscapeDataString(inputText)}");

        // Ensure the request was successful
        response.EnsureSuccessStatusCode();

        // Load HTML response
        var htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(await response.Content.ReadAsStringAsync());

        // Parse HTML to find the desired node
        var node = htmlDocument.DocumentNode.SelectSingleNode("//div[@id='color']");

        // Return HTML content
        var innerHtml = node?.InnerHtml;
        var innerHtmlWithWhitespaceRemoved = RemoveUnnecessaryWhitespace(innerHtml);
        return innerHtmlWithWhitespaceRemoved;
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex MultipleWhitespacesRegex();

    private string RemoveUnnecessaryWhitespace(string? input)
    {
        if (input is null) return null;

        var output = MultipleWhitespacesRegex().Replace(input.Trim(), " ");
        return output;
    }
}
