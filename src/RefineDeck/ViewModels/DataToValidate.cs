using PropertyChanged;

namespace RefineDeck.ViewModels;

[AddINotifyPropertyChangedInterface]
public record DataToValidate
{
    public string FrontSide_Question { get; init; }
    public string BackSide_Answer { get; init; }
    public string SentenceExample { get; init; }
    public string SentenceExampleTranslation { get; init; }
    public string RemarksFromTeacherToStudent { get; init; }
}
