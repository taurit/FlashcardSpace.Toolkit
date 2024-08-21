using AnkiCardValidator.Models;
using AnkiCardValidator.Utilities;
using PropertyChanged;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace AnkiCardValidator.ViewModels;

[AddINotifyPropertyChangedInterface]
[DebuggerDisplay("{CardDirectionFlag} {Question} -> {Answer}")]
public sealed class CardViewModel(
    // raw source data
    AnkiNote note,
    bool isReverseCard,

    // derived from source data locally
    FlashcardDirection noteDirection,
    int? frequencyPositionQuestion,
    int? frequencyPositionAnswer,
    int numDefinitionsForQuestion,
    int numDefinitionsForAnswer,

    // derived from source data using ChatGPT
    CefrClassification cefrLevelQuestion,
    string? qualityIssues,
    string? rawResponseFromChatGptApi
)
{
    // reference to the evaluated note
    public AnkiNote Note { get; } = note;

    // quality signals calculated locally
    public FlashcardDirection NoteDirection { get; } = noteDirection;

    [DependsOn(nameof(NoteDirection))]
    public bool IsQuestionInPolish => ((NoteDirection == FlashcardDirection.FrontTextInPolish && !isReverseCard) || (NoteDirection == FlashcardDirection.FrontTextInSpanish && isReverseCard));

    [DependsOn(nameof(IsQuestionInPolish))]
    public string CardDirectionFlag => IsQuestionInPolish
        ? "\ud83c\uddf5\ud83c\uddf1" // Polish flag emoji
        : "\ud83c\uddea\ud83c\uddf8" // Spanish flag emoji
    ;

    [DependsOn(nameof(Note))]
    public string Question => isReverseCard ? Note.BackText : Note.FrontText;

    [DependsOn(nameof(Note))]
    public string QuestionAudio => isReverseCard ? Note.BackAudio : Note.FrontAudio;

    [DependsOn(nameof(Note))]
    public string Answer => isReverseCard ? Note.FrontText : Note.BackText;

    [DependsOn(nameof(Note))]
    public string AnswerAudio => isReverseCard ? Note.FrontAudio : Note.BackAudio;

    /// <summary>
    /// My convention during review, is to add the
    /// "todo {comment}"
    /// annotation to the cards in Anki if they require clarification (usually I add it in the Examples field).
    /// </summary>
    [DependsOn(nameof(Note))]
    public bool ContainsTodoAnnotation => note.Remarks.StartsWith("todo", StringComparison.InvariantCultureIgnoreCase);
    // todo improve detection - both false positives and negatives

    public int? FrequencyPositionQuestion { get; } = frequencyPositionQuestion;
    public int? FrequencyPositionAnswer { get; } = frequencyPositionAnswer;

    public int NumDefinitionsForQuestion { get; } = numDefinitionsForQuestion;
    public int NumDefinitionsForAnswer { get; } = numDefinitionsForAnswer;

    public ObservableCollection<CardViewModel> DuplicatesOfQuestion { get; } = [];

    // data received from ChatGPT
    public string? RawResponseFromChatGptApi { get; set; } = rawResponseFromChatGptApi;

    public CefrClassification CefrLevelQuestion { get; set; } = cefrLevelQuestion;

    public string? QualityIssuesRaw { get; set; } = qualityIssues;

    // Refined value of `QualityIssuesRaw` to be displayed in the UI.
    [DependsOn(nameof(QualityIssuesRaw))]
    public string? QualityIssues =>
        (QualityIssuesRaw is not null &&
         (
             QualityIssuesRaw.StartsWith("None") ||
             QualityIssuesRaw.StartsWith("Fine") ||
             QualityIssuesRaw.StartsWith("No issues") ||
             QualityIssuesRaw.StartsWith("No notable issues") ||
             QualityIssuesRaw.StartsWith("No significant issues") ||
             QualityIssuesRaw.StartsWith("All good") ||
             QualityIssuesRaw.StartsWith("No need for extra notes") ||
             QualityIssuesRaw.StartsWith("Duplicate flashcard") ||
             QualityIssuesRaw.StartsWith("Duplicate entry") ||

             // my old ChatGPT query was raising issues about HTML tags being used; try to filter them out (at a risk I'll skip some other issues)
             QualityIssuesRaw.Contains("HTML") ||
             QualityIssuesRaw.Contains(" tags ") ||
             QualityIssuesRaw.Contains(" markup ") ||
             QualityIssuesRaw.Contains("Formatting issues") ||
             QualityIssuesRaw.Contains("Formatting should be") ||

             // also this, before I had proper detection of the card direction:
             QualityIssuesRaw.Contains("Polish instead of ") ||
             QualityIssuesRaw.Contains("instead of a Spanish") ||
             QualityIssuesRaw.Contains("Polish, not ") ||
             QualityIssuesRaw.Contains("should be in Spanish") ||
             QualityIssuesRaw.Contains("should be Spanish") ||
             QualityIssuesRaw.Contains("contains Polish") ||
             QualityIssuesRaw.Contains("mostly Polish") ||

             // some of my imported cards contain * at the end, but it's a task for duplicate detection
             QualityIssuesRaw.Contains("asterisk") ||
             QualityIssuesRaw.Contains("*") ||
             QualityIssuesRaw.Contains("multi-word phrase") ||

             // some suggestions are that term is suitable for B1/B2, but I have this handled with another mechanism
             QualityIssuesRaw.Contains(" B1") ||
             QualityIssuesRaw.Contains(" B2") ||
             QualityIssuesRaw.Contains(" valid for basic ") ||
             QualityIssuesRaw.Contains("no issues") ||
             QualityIssuesRaw.Contains("seems accurate") ||
             QualityIssuesRaw.Contains("accurately represents") ||

             // I have a separate mechanism to reliably detect duplicates in entire Anki collection, so skip such issues
             QualityIssuesRaw.Contains("duplicate")
         )
        )
            ? ""
            : QualityIssuesRaw;

    public ObservableCollection<Meaning> Meanings { get; init; } = [];

    // data derived from ChatGPT response
    [DependsOn(nameof(QualityIssues))] private bool HasQualityIssues => !String.IsNullOrWhiteSpace(QualityIssues);

    [DependsOn(nameof(CefrLevelQuestion), nameof(HasQualityIssues), nameof(Meanings), nameof(NumDefinitionsForQuestion), nameof(NumDefinitionsForAnswer))]
    public int Penalty =>
        // missing information about CEFR level
        (CefrLevelQuestion == CefrClassification.Unknown ? 1 : 0) +

        // words with CEFR level C1 and higher should be prioritized down until I learn basics
        (CefrLevelQuestion >= CefrClassification.C1 ? 1 : 0) +

        // words with CEFR level C2 should be prioritized down even more than B2
        (CefrLevelQuestion >= CefrClassification.C2 ? 1 : 0) +

        // the more individual meanings word has, the more confusing learning it with flashcards might be
        (Meanings.Count > 0 ? Meanings.Count - 1 : 0) +

        // ChatGPT raised at least one quality issue
        (HasQualityIssues ? 2 : 0) +

        // Question appears to have duplicates in the Anki collection
        DuplicatesOfQuestion.Count +

        // number of terms on the side of the flashcard. For example, if the front contains text 'mnich, zakonnik', this will be 2
        // (the ideal number is 1)
        (NumDefinitionsForQuestion - 1) +
        (NumDefinitionsForAnswer - 1) +

        // no frequency data - this can be false negative, if term is a sentence, or HTML tags weren't sanitized.
        // I can improve false alarms with heuristics
        //(FrequencyPositionQuestion.HasValue ? 0 : 2) +
        //(FrequencyPositionAnswer.HasValue ? 0 : 2) +
        // I decided to no longer punish for missing frequency data, as plenty flashcards consist of multiple words ("de nada", "por favor", ...)
        // and they are fine. 

        // frequency data exists and suggests that Spanish word is used very infrequently
        (FrequencyPositionQuestion.HasValue ? CalculateFrequencyPenalty(FrequencyPositionQuestion.Value) : 0) +

        // same for polish side
        (FrequencyPositionAnswer.HasValue ? CalculateFrequencyPenalty(FrequencyPositionAnswer.Value) : 0) +


        (ContainsTodoAnnotation ? 1 : 0)
        ;

    private int CalculateFrequencyPenalty(int position) => position switch
    {
        < 10000 => 0,
        < 20000 => 1,
        < 30000 => 2,
        < 40000 => 3,
        _ => 4
    };
}
