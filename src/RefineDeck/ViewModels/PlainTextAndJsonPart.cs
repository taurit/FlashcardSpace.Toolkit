using PropertyChanged;

namespace RefineDeck.ViewModels;

[AddINotifyPropertyChangedInterface]
public record PlainTextAndJsonPart(string PlainText, DataToValidate? Suggestion);
