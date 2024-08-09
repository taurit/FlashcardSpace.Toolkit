using AnkiCardValidator;
using AnkiCardValidator.Utilities;
using AnkiCardValidator.Utilities.JsonGenerativeFill;
using AnkiCardValidator.ViewModels;
using Spectre.Console;

namespace UpdateField.Mutations;
internal static class AddPolishTranslationToRemarks
{
    const string RemarkId = "pl";
    const string SystemChatMessage = "You are an assistant who helps students of Ukrainian language improve their flashcards. Students already know Polish and English.";

    public static List<AnkiNote> LoadNotesThatRequireAdjustment()
    {
        // Load notes that need adjustment
        var notes = AnkiHelpers.GetNotes(Settings.AnkiDatabaseFilePath, limitToTag: "s06")
            .Where(x => !x.Remarks.HasRemark(RemarkId))
            .ToList();
        return notes;
    }


    class FlashcardToFill : ItemWithId
    {
        public string Ukrainian { get; set; }
        public string English { get; set; }

        [FilledByAI]
        public string Polish { get; set; }
    }

    public static async Task AddPolishTranslation(List<AnkiNote> notesChunk)
    {
        if (notesChunk.Count > 40)
            throw new NotImplementedException("Probably too many notes to successfully process in one prompt/call. Did you forget to chunk the collection?");

        AnsiConsole.WriteLine($"Fetching translations from ChatGPT API for a chunk of {notesChunk.Count} notes...");

        var fillModel = notesChunk.Select(x => new FlashcardToFill()
        {
            Ukrainian = x.FrontText,
            English = x.BackText
        }).ToList();

        var filledNotes = await JsonGenerativeFill.FillMissingProperties(fillModel,
            "- Given input in Ukrainian and usage context in English, provide the closest equivalent of Ukrainian content in Polish. Be as accurate as possible.\n" +
            "- English translation or comment is only provided to clarify the context.\n" +
            "- The output value in 'Polish' should be ready to put onto the reverse side of the flashcard, so don't be verbose (provide a brief translation). Stick to plain text.\n" +
            "- If you encounter Anki tags like <img/> or [sound:...], ignore them as if they weren't in the input.\n" +
            "- Many cards contain two words - the imperfective and perfective form of verbs. In such case, provide translations of both to Polish.",
            SystemChatMessage);

        foreach (var filledNote in filledNotes)
        {
            var noteToUpdate = notesChunk.Single(x => x.FrontText == filledNote.Ukrainian);
            var updatedRemarks = noteToUpdate.Remarks.AddOrUpdateRemark(RemarkId, filledNote.Polish);
            noteToUpdate.Remarks = updatedRemarks;
        }

    }
}
