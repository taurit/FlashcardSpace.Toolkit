using CoreLibrary.Utilities;
using PropertyChanged;

namespace RefineDeck.ViewModels;

[AddINotifyPropertyChangedInterface]
public record PlainTextAndJsonPart(string RawResponse, string PlainText, DataToValidate? Suggestion)
{
    public string Fingerprint => RawResponse.GetHashCodeStable(16);
}
