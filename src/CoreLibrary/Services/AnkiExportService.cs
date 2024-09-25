using Anki.NET;
using Anki.NET.Models;
using Anki.NET.Models.Scriban;
using CoreLibrary.Models;

namespace CoreLibrary.Services;

public class AnkiExportService
{
    /// <summary>
    /// A list of fields striving to be universal for all Anki decks, regardless of source and target languages.
    /// Some things I might want to add in anticipation of future needs, even though not necessary today:
    /// - fields for "culture facts and trivia" cards
    /// - field for third language (beside source language and target language); this could be helpful for people
    ///   who learn their 3rd language and already know two.
    /// </summary>
    private static readonly FieldList Fields = new()
    {
        new("FrontText"),
        new("FrontAudio"),
        new("BackText"),
        new("BackAudio"),
        new("Image"),
        new("SentenceExample"),
        new("SentenceExampleAudio"),
        new("SentenceExampleTranslation"),
        new("Remarks"),
    };

    private static readonly string FrontSideTemplate =
            // wrap question in a div to make it easier to style
            "<div class=\"frontText\">{{FrontText}}</div>\n" +
            // render audio tag only if the audio is present
            "{{#FrontAudio}}<br />{{FrontAudio}}{{/FrontAudio}}"
        ;

    private static readonly string BackSideTemplate =
            "{{FrontSide}}\n" +
            "<hr id=answer>\n" +

            "<div class=\"backText\">{{BackText}}</div>\n" +
            "{{#BackAudio}}<br />{{BackAudio}}{{/BackAudio}}\n" +

            // image
            "{{#Image}}<br />{{Image}}{{/Image}}\n" +

            // sentence example
            "{{#SentenceExample}}" +
            "<div class=\"sentenceSourceLanguage\">{{SentenceExample}}</div>\n" +
            "{{#SentenceExampleAudio}}<br />{{SentenceExampleAudio}}{{/SentenceExampleAudio}}\n" +
            "<div class=\"sentenceTargetLanguage\">{{SentenceExampleTranslation}}</div>\n" +
            "{{/SentenceExample}}" +

            // remarks
            "{{#Remarks}}<br /><div class=\"remarks\">{{Remarks}}</div>{{/Remarks}}"
            ;

    public void ExportToAnki(string editableDeckFolderPath, string outputFolderPath)
    {
        var manifestFilePath = Path.Combine(editableDeckFolderPath, "flashcards.edit.json");
        if (!File.Exists(manifestFilePath))
        {
            manifestFilePath = Path.Combine(editableDeckFolderPath, "flashcards.json");

            if (!File.Exists(manifestFilePath))
            {
                throw new FileNotFoundException($"Could not find the flashcards.edit.json not flashcards.json in the specified folder ({editableDeckFolderPath}).");
            }
        }
        var manifestFolder = Path.GetDirectoryName(manifestFilePath)!;

        var deck = Deck.DeserializeFromFile(manifestFilePath);

        var cardTemplates = new[] {
            new CardTemplate(0, "Forward", FrontSideTemplate, BackSideTemplate)
        };

        var ankiNoteModel = new AnkiDeckModel(deck.DeckName, Fields, cardTemplates, deck.MediaFilesPrefix);
        var exportedDeck = new AnkiDeck(ankiNoteModel);

        foreach (var flashcard in deck.Flashcards)
        {
            var imageTag = RegisterImageAndGetImageTag(exportedDeck, manifestFolder, flashcard);

            string? termAudioTag = RegisterAudioAndGetSoundTag(exportedDeck, manifestFolder,
                flashcard.Overrides?.TermAudio, flashcard.TermAudio);

            string? termTranslationAudioTag = RegisterAudioAndGetSoundTag(exportedDeck, manifestFolder,
                flashcard.Overrides?.TermTranslationAudio, flashcard.TermTranslationAudio);

            string? contextAudioTag = RegisterAudioAndGetSoundTag(exportedDeck, manifestFolder,
                flashcard.Overrides?.ContextAudio, flashcard.ContextAudio);

            // add card to the deck
            string[] noteFields =
            [
                flashcard.Overrides?.Term ?? flashcard.Term, // FrontText
                termAudioTag, // FrontAudio
                flashcard.Overrides?.TermTranslation ?? flashcard.TermTranslation, // BackText
                termTranslationAudioTag, // BackAudio
                imageTag, // Image
                flashcard.Overrides?.Context ?? flashcard.Context, // SentenceExample
                contextAudioTag, // SentenceExampleAudio

                flashcard.Overrides?.ContextTranslation ?? flashcard.ContextTranslation, // SentenceExampleTranslation
                flashcard.Overrides ?.TermDefinition ?? flashcard.TermDefinition // Remarks
            ];

            if (noteFields.Length != Fields.Count)
            {
                throw new InvalidOperationException($"The number of fields in the note ({noteFields.Length}) does not match the number of fields in the deck ({Fields.Count}).");
            }

            exportedDeck.AddItem(noteFields);
        }

        exportedDeck.CreateApkgFile(outputFolderPath);
    }

    private static string RegisterImageAndGetImageTag(AnkiDeck exportedDeck, string manifestFileFolder,
        FlashcardNote flashcard)
    {
        string? imageFileNameDeck = null;

        if (flashcard.SelectedImageIndex != null)
        {
            bool imageIsPresent = flashcard.SelectedImageIndex != null &&
                                  flashcard.SelectedImageIndex >= 0 &&
                                  flashcard.SelectedImageIndex < flashcard.ImageCandidates.Count;

            if (imageIsPresent)
            {
                var imageFilePathRelative = flashcard.ImageCandidates[flashcard.SelectedImageIndex!.Value];
                var imageFilePathAbsolute = Path.Combine(manifestFileFolder, imageFilePathRelative);
                imageFileNameDeck = exportedDeck.RegisterImageFile(imageFilePathAbsolute);
            }
        }
        var imageTag = String.IsNullOrWhiteSpace(imageFileNameDeck) ? "" : $"<img src=\"{imageFileNameDeck}\" />";
        return imageTag;
    }

    private string RegisterAudioAndGetSoundTag(AnkiDeck exportedDeck, string manifestFileFolder, string? termAudioOverride, string termAudioBase)
    {
        var termAudio = termAudioOverride ?? termAudioBase;

        if (!String.IsNullOrWhiteSpace(termAudio))
        {
            var audioFilePathAbsolute = Path.Combine(manifestFileFolder, termAudio);
            var deckPath = exportedDeck.RegisterAudioFile(audioFilePathAbsolute);

            var soundTag = $"[sound:{deckPath}]";
            return soundTag;
        }
        return String.Empty;
    }
}
