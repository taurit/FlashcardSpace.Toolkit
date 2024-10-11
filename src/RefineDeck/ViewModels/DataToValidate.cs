using PropertyChanged;

namespace RefineDeck.ViewModels;

[AddINotifyPropertyChangedInterface]
public record DataToValidate
{
    public string FrontSide_QuestionInSpanish { get; init; }
    public string BackSide_AnswerInPolish { get; init; }
    public string BackSide_SentenceExampleInSpanish { get; init; }
    public string BackSide_SentenceExampleTranslationToPolish { get; init; }
    public string BackSide_RemarksFromTeacherToStudent { get; init; }
}
