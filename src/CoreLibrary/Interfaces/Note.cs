namespace CoreLibrary.Interfaces;

public record Note(string Term, string Sentence, PartOfSpeech PartOfSpeech, Dictionary<string, string> Fields);
