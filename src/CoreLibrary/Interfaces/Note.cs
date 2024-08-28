namespace CoreLibrary.Interfaces;

public record Note(
    string Term,
    string Sentence,
    string WordBaseForm,
    PartOfSpeech PartOfSpeech,
    Dictionary<string, string> OtherFields);
