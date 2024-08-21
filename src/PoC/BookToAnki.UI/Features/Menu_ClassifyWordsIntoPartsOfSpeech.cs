using BookToAnki.UI.ViewModels;
using System;
using System.IO;
using System.Linq;
using System.Windows;

namespace BookToAnki.UI;

public partial class MainWindow
{
    private async void ClassifyWordsIntoPartsOfSpeech_OnClick(object sender, RoutedEventArgs e)
    {

        var lastProcessedWord = "шугав"; // temporary hack if process crashed at some point

        var wordsToClassify = Enumerable
            .OfType<WordDataViewModel>(WordsDataGrid.Items)
            .ToList();

        var lastProcessedWordPosition = 0; //wordsToClassify.FindIndex(x => x.Word.Word == lastProcessedWord);

        var wordsLeftToClassify = wordsToClassify
            .Skip(lastProcessedWordPosition)
            .Where(x => x.PartOfSpeech is null)
            .Select(x => x.Word.Word)
            .ToList();

        var chunks = wordsLeftToClassify.Chunk(50).ToList();

        var systemPrompt = await File.ReadAllTextAsync("Resources/CategorizeWords.gpt35optimized.txt");

        foreach (var chunk in chunks)
        {
            var userPrompt = "```txt\n" +
                             string.Join(Environment.NewLine, chunk) +
                             "\n" +
                             "```";

            var response = await _openAiService.CreateChatCompletion(systemPrompt, userPrompt,
                OpenAI.ObjectModels.Models.Gpt_4o, false);
            ViewModel.TotalCostUsdNumber = _openAiService.TotalCostUsd;

            await File.AppendAllTextAsync("d:\\Flashcards\\Words\\ukrainian_parts_of_speech_automated.txt", $"\n{response}\n");
        }
    }
}
