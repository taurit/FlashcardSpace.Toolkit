using CoreLibrary;
using CoreLibrary.Interfaces;

namespace ReferenceImplementations;

public class ReferenceTermExtractor : IExtractTerms
{
    public async Task<List<Note>> ExtractTerms(List<string> sentences)
    {
        var notes = new List<Note>();

        foreach (var sentence in sentences)
        {
            var words = sentence.Split([' ', '\n', '\r', '\t', ',', ';'], StringSplitOptions.RemoveEmptyEntries);
            notes.AddRange(words.Select(word => new Note(word, sentence, PartOfSpeech.Unknown)));
        }

        return notes;
    }
}
