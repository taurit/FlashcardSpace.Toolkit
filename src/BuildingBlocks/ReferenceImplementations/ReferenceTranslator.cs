using CoreLibrary;
using CoreLibrary.Interfaces;

namespace ReferenceImplementations;

public class ReferenceTranslator : IProvideFieldValues
{
    public async Task<List<Note>> ProcessNotes(List<Note> notes)
    {
        var outputNotes = new List<Note>();

        foreach (var note in notes)
        {
            var fieldsCopy = new Dictionary<string, string>(note.Fields);

            // simulate translation to a fake language "YellingEnglish" (ALL CAPS), without calling any external APIs
            fieldsCopy["YellingEnglishTranslation"] = note.Term.ToUpperInvariant();
            fieldsCopy["YellingEnglishDefinition"] = $"SOME DEFINITION OF THE WORD `{note.Term.ToUpperInvariant()}`";

            var extendedNote = note with { Fields = fieldsCopy };
            outputNotes.Add(extendedNote);
        }

        return outputNotes;
    }
}
