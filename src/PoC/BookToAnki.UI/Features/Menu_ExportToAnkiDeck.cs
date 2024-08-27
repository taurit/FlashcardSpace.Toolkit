using Anki.NET;
using Anki.NET.Models;
using Anki.NET.Models.Scriban;
using BookToAnki.Models;
using BookToAnki.Services;
using BookToAnki.UI.ViewModels;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace BookToAnki.UI;

public partial class MainWindow
{
    private async void ExportToAnkiDeckAuto_OnClick(object sender, RoutedEventArgs e)
    {
        // Select examples to export to a deck
        var words = WordsDataGrid.Items
            .OfType<WordDataViewModel>()
            .Select(x => x.Word)
            .Take(3000)
            .ToList();

        var examplesToExport = new List<WordUsageExample>();
        var wordsToExplain = new List<WordToExplain>(words.Count);
        foreach (var word in words)
        {
            var usageExamplesToUse = word
                    .UsageExamples
                    .Where(x => x.QualityPenalty <= 1)
                    .Where(x => x.SentenceHumanTranslationPolish is not null) // covered by quality penalty, but just in case
                    .OrderBy(x => x.QualityPenalty)
                    .Take(3) // for some words I might have 1000s example sentences, but it seems reasonable to include only a few to the deck
                    .ToList()
                ;

            examplesToExport.AddRange(usageExamplesToUse);
            if (usageExamplesToUse.Any())
                wordsToExplain.Add(new WordToExplain(word.Word, usageExamplesToUse[0].Sentence.Text));
        }

        // make sure we have all data from ChatGPT, in a cost-effective way
        // 30 = 1 PLN using GPT4 API 
        //await _ukrainianWordExplainer.BatchPrepareExplanations(wordsToExplain, 10 * 30);

        await CreateDeck("Ukrainian - understanding Harry Potter", "hp", examplesToExport);
    }

    private async Task CreateDeck(string deckName, string prefixForMedia, List<WordUsageExample> usageExamplesToExport)
    {
        var fieldList = new FieldList
        {
            new("UkrainianSentence"),
            new("UkrainianSentenceAudio"),
            new("UkrainianNominative"),
            new("PolishNominative"),
            new("PolishExplanation"),
            new("PolishSentence"),
            new("Illustration")
        };
        CardTemplate[] cardTemplates =
        [
            new(0, "RecognizeWordInSentence",
                "{{UkrainianSentence}}<br />\n" +
                "{{UkrainianSentenceAudio}}",
                "{{FrontSide}}\n" +
                "<hr id=answer>" +
                "{{UkrainianNominative}} = {{PolishNominative}}<br />\n" +
                "<br />\n" +
                "{{PolishExplanation}}<br />\n" +
                "<br />\n" +
                "<i>{{PolishSentence}}</i><br /><br />\n" +
                "{{Illustration}}")
        ];
        var deckModel =
            new AnkiDeckModel(deckName, fieldList, cardTemplates, prefixForMedia);
        var deck = new AnkiDeck(deckModel);
        foreach (var exampleToExplain in usageExamplesToExport)
        {
            var imageFolderPath =
                Path.Combine(Settings.ImagesRepositoryFolder, exampleToExplain.Word).ToLowerInvariant();
            var ankiNote = await _ankiNoteGenerator.GenerateAnkiNote(exampleToExplain, imageFolderPath);

            var imageField = " ";
            if (ankiNote.ExplanationImageFilePath is not null)
            {
                var imageReferenceInDeck = deck.RegisterImageFile(ankiNote.ExplanationImageFilePath);
                imageField = $"<img src=\"{imageReferenceInDeck}\" />";
            }

            var fullPathToAudioFile =
                Path.Combine(Settings.AudioFilesCacheFolder, ankiNote.UkrainianSentenceAudioFileName);
            var audioReferenceInDeck = deck.RegisterAudioFile(fullPathToAudioFile);
            var audioField = $"[sound:{audioReferenceInDeck}]";

            deck.AddItem(
                ankiNote.UkrainianSentenceWithStressesAndWordHighlighted,
                audioField,
                ankiNote.NominativeForm,
                ankiNote.BestWordEquivalentInPolish,
                ankiNote.WordExplanationInPolish,
                ankiNote.SentenceEquivalentInPolish,
                imageField);
        }

        var createdDeckFilePath = deck.CreateApkgFile("d:\\TestDeck");
        MessageBox.Show($"{createdDeckFilePath}.", "Anki Deck was created successfully", MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

}
