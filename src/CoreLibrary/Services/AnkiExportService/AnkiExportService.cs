using Anki.NET;
using Anki.NET.Helpers;
using Anki.NET.Models;
using Anki.NET.Models.Scriban;
using CoreLibrary.Models;

namespace CoreLibrary.Services.AnkiExportService;

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

        // hack needed -> I need just filename in template to use my own player view, but I also 
        // need the unused [sound:...] tag to make Anki import the file to media collection.
        new("SentenceExampleAudio"),
        new("SentenceExampleAudioFileName"),

        new("SentenceExampleTranslation"),
        new("Remarks"),
    };

    private static readonly string FrontSideTemplate =
            // wrap question in a div to make it easier to style
            "<div class=\"frontText\">{{FrontText}}</div>\n" +
            // render audio tag only if the audio is present
            "{{#FrontAudio}}{{FrontAudio}}{{/FrontAudio}}"
        ;


    private static readonly string CardStyleCss = GeneralHelper.ReadResource("CoreLibrary.Services.AnkiExportService.CardStyle.css");
    private static readonly string CardScriptJs = GeneralHelper.ReadResource("CoreLibrary.Services.AnkiExportService.CardScript.js");


    private static readonly string BackSideTemplate =
            "{{FrontSide}}\n" +
            "<hr id=answer>\n" +

            "<div class=\"backText\">{{BackText}}</div>\n" +
            "{{#BackAudio}}{{BackAudio}}{{/BackAudio}}\n" +

            // image
            "{{#Image}}<br /><div class=\"illustration\">{{Image}}</div>{{/Image}}\n" +

            // sentence example
            "{{#SentenceExample}}" +
            "<div id=\"sentenceSourceLanguage\">{{SentenceExample}}</div>\n" +
            "<audio id=\"sentenceSourceLanguageAudio\" preload=\"auto\" src=\"{{SentenceExampleAudioFileName}}\"></audio>\n" +
            "<div id=\"sentenceTargetLanguage\">{{SentenceExampleTranslation}}</div>\n" +
            "{{/SentenceExample}}" +

            // remarks
            "{{#Remarks}}<br /><div class=\"remarks\">{{Remarks}}</div>{{/Remarks}}\n\n" +

            // script
            "<script>\n" +
            "(function() {\n" +
            CardScriptJs +
            "\n" +
            "})();\n" +
            "</script>\n"
            ;


    public void ExportToAnki(DeckPath deckPath)
    {
        var manifestFilePath = deckPath.DeckManifestEditsPathWithFallback;
        if (!File.Exists(manifestFilePath))
        {
            throw new FileNotFoundException($"Could not find the flashcards.edited.json not flashcards.json in ({deckPath.DeckDataPath}).");
        }
        var manifestFolder = Path.GetDirectoryName(manifestFilePath)!;

        var deck = Deck.DeserializeFromFile(manifestFilePath);

        var cardTemplates = new[] {
            new CardTemplate(0, "Forward", FrontSideTemplate, BackSideTemplate)
        };

        var ankiNoteModel = new AnkiDeckModel(deck.DeckName, Fields, cardTemplates, deck.MediaFilesPrefix, CardStyleCss);
        var exportedDeck = new AnkiDeck(ankiNoteModel);

        var flashcardsToExport = deck.Flashcards.Where(x => x.ApprovalStatus == ApprovalStatus.Approved).ToList();
        foreach (var flashcard in flashcardsToExport)
        {
            var imageTag = RegisterImageAndGetImageTag(exportedDeck, manifestFolder, flashcard);

            var termAudioTag = RegisterAudioAndGetSoundTag(exportedDeck, manifestFolder,
                flashcard.Overrides?.TermAudio, flashcard.TermAudio);

            var termTranslationAudioTag = RegisterAudioAndGetSoundTag(exportedDeck, manifestFolder,
                flashcard.Overrides?.TermTranslationAudio, flashcard.TermTranslationAudio);

            var contextAudioTag = RegisterAudioAndGetSoundTag(exportedDeck, manifestFolder,
                flashcard.Overrides?.ContextAudio, flashcard.ContextAudio);

            // add card to the deck
            string[] noteFields =
            [
                flashcard.Overrides?.Term ?? flashcard.Term, // FrontText
                termAudioTag.FileNameInTag, // FrontAudio
                flashcard.Overrides?.TermTranslation ?? flashcard.TermTranslation, // BackText
                termTranslationAudioTag.FileNameInTag, // BackAudio
                imageTag, // Image
                flashcard.Overrides?.Context ?? flashcard.Context, // SentenceExample
                
                contextAudioTag.FileNameInTag, // SentenceExampleAudio
                contextAudioTag.FileName, // SentenceExampleAudioFileName

                flashcard.Overrides?.ContextTranslation ?? flashcard.ContextTranslation, // SentenceExampleTranslation
                flashcard.Overrides?.Remarks ?? flashcard.Remarks // Remarks
            ];

            if (noteFields.Length != Fields.Count)
            {
                throw new InvalidOperationException($"The number of fields in the note ({noteFields.Length}) does not match the number of fields in the deck ({Fields.Count}).");
            }

            exportedDeck.AddItem(noteFields);
        }

        exportedDeck.CreateApkgFile(deckPath.AnkiExportPath);
    }

    private static string RegisterImageAndGetImageTag(AnkiDeck exportedDeck, string manifestFileFolder,
        FlashcardNote flashcard)
    {
        string? imageFileNameDeck = null;

        var selectedImageIndex = flashcard.Overrides?.SelectedImageIndex ?? flashcard.SelectedImageIndex;

        if (selectedImageIndex != null)
        {
            var selectedImageIndexValue = selectedImageIndex.Value;
            var imageIsPresent = selectedImageIndexValue >= 0 &&
                                 selectedImageIndexValue < flashcard.ImageCandidates.Count;

            if (imageIsPresent)
            {
                var imageFilePathRelative = flashcard.ImageCandidates[selectedImageIndexValue];
                var imageFilePathAbsolute = Path.Combine(manifestFileFolder, imageFilePathRelative);
                imageFileNameDeck = exportedDeck.RegisterImageFile(imageFilePathAbsolute);
            }
        }
        var imageTag = string.IsNullOrWhiteSpace(imageFileNameDeck) ? "" : $"<img src=\"{imageFileNameDeck}\" />";
        return imageTag;
    }

    public record SoundMediaReference(string FileName)
    {
        public string FileNameInTag => $"[sound:{FileName}]";
    }

    private SoundMediaReference? RegisterAudioAndGetSoundTag(AnkiDeck exportedDeck, string manifestFileFolder, string? termAudioOverride, string termAudioBase)
    {
        var termAudio = termAudioOverride ?? termAudioBase;

        if (!string.IsNullOrWhiteSpace(termAudio))
        {
            var audioFilePathAbsolute = Path.Combine(manifestFileFolder, termAudio);
            var soundFilePath = exportedDeck.RegisterAudioFile(audioFilePathAbsolute);
            return new SoundMediaReference(soundFilePath);
        }
        return null;
    }

}
