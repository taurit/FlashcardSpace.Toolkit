using BookToAnki.NotePropertiesDatabase;

namespace BookToAnki.Models;

public record UkrainianAnkiNote(PrefKey PrefKey,
    string UkrainianWordWithAccentHighlighted,
    string UkrainianSentenceWithStressesAndWordHighlighted,
    string UkrainianSentenceAudioFileName,
    string NominativeForm,
    string NominativeFormHighlighted,
    string BestWordEquivalentInPolish,
    string WordExplanationInPolish,
    string BestWordEquivalentInEnglish,
    string WordExplanationInEnglish,
    // ideally, matched from human translation
    string? SentenceEquivalentInEnglish,
    string? SentenceEquivalentInPolish,
    string? ExplanationImageFilePath,
    SentenceWithSound SelectedAudioFile,
    List<SentenceWithSound> AllPossibleAudioFiles,
    Rating? Rating,
    string? SentenceMachineTranslationToPl
)
{
    internal string? ExplanationImageFileName =>
        string.IsNullOrEmpty(ExplanationImageFilePath)
            ? null
            : Path.GetFileName(ExplanationImageFilePath);

    public AnkiCard UkrainianToPolishCard
    {
        get
        {
            var question =
                    //$"<strong>{UkrainianWordWithAccentHighlighted}</strong><br /><br />" +
                    $"{UkrainianSentenceWithStressesAndWordHighlighted}<br /><br />" +
                    $"<audio id=\"myAudio\" src=\"http://localhost:8080/.Cache/AudioSentences/{UkrainianSentenceAudioFileName}\" preload autoplay controls style=\"height: 32px; width: 190px;\"></audio><br /> {Editable(EditableField.SelectedAudioTrim, "\u2702\ufe0f")}"
                ;

            var answer =
                    $"<strong>{Editable(EditableField.OriginalNominative, NominativeFormHighlighted)}</strong> = {Editable(EditableField.PolishWordTranslation, BestWordEquivalentInPolish)}<br />" +
                    $"<i>{Editable(EditableField.PolishSentenceTranslation, SentenceEquivalentInPolish)}</i><br /><br />" +

                    $"def.: {Editable(EditableField.PolishWordExplanation, WordExplanationInPolish)}<br /><br />" +


                    $" " +

                    (ExplanationImageFileName != null
                        ? $"<br />{Editable(EditableField.SelectedImage, $"<img src=\"http://localhost:8080/Pictures/{WordFolder}/{ExplanationImageFileName}\" style=\"max-width: 400px;\" /><br />")}"
                        : "")
                ;

            return new AnkiCard(question, answer);
        }
    }

    private string Editable(EditableField field, string? markupToWrap)
    {
        if (String.IsNullOrWhiteSpace(markupToWrap))
        {
            markupToWrap = "\u270d\ufe0f";
        }

        return $"<span onclick=\"send('{field}')\" style=\"cursor: pointer;\">{markupToWrap}</span>";
    }

    private string? WordFolder
    {
        get
        {
            if (ExplanationImageFilePath is null) return null;
            var directoryPath = Path.GetDirectoryName(ExplanationImageFilePath);
            if (directoryPath == null) return null;
            return directoryPath.Split(Path.DirectorySeparatorChar).Last();
        }
    }
}
