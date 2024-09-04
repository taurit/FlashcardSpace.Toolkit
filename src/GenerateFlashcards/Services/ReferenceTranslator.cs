namespace GenerateFlashcards.Services;

public class ReferenceTranslator : IProvideTranslations
{
    public async Task<List<Note>> AddTranslations(List<TermInContext> terms)
    {
        var outputNotes = new List<Note>();

        foreach (var term in terms)
        {

            var newNote = new Note(term);

            // simulate translation to a fake language "YellingEnglish" (ALL CAPS), without calling any external APIs
            var termInTargetLanguage = term.TermOriginal.ToUpperInvariant();
            var definitionInTargetLanguage = $"DEFINITION OF `{term.TermOriginal.ToUpperInvariant()}`";

            outputNotes.Add(newNote);
        }

        return outputNotes;
    }
}
