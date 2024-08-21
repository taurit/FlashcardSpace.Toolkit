namespace BookToAnki.Models;

public record AnkiCard(string QuestionHtml, string AnswerHtml)
{
    public string PreviewHtml =>
        // set up messaging from the html to app so edit buttons can work
        $"<script>function send(msg) {{ window.chrome.webview.postMessage(msg); }}</script>" +

        $"<div style=\"text-align: center; font-family: 'Calibri'; font-size: 1.2em;\">{QuestionHtml}</div>" +
        $"<hr />" +
        $"<div style=\"text-align: center; font-family: 'Calibri'; font-size: 1.2em;\">{AnswerHtml}</div>";
}
